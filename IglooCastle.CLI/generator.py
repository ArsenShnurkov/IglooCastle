﻿import sys
import clr
import System
from time import gmtime, strftime

clr.AddReference("IglooCastle.CLI")
import IglooCastle.CLI
clr.ImportExtensions(IglooCastle.CLI)


def flatten(something):
	for x in something:
		if isinstance(x, list):
			for y in flatten(x):
				yield y
		else:
			yield x

def escape(str):
	return str.replace("`", "%60")

def a(href, text):
	return '<a href="%s">%s</a>' % (escape(href), text)

def fmt_non_empty(template, contents):
	"""Formats the contents into template, if the contents are not empty."""
	if len(contents):
		return template % contents
	else:
		return ""

def filter_empty(node_list):
	return [ n for n in node_list if not n.is_content_empty() ]

class HtmlTemplate:
	def __init__(self):
		self.title  = ""
		self.h1     = ""
		self.nav    = ""
		self.main   = ""
		self.footer = ""

	def write(self, file):
		print "Writing file %s" % file
		f = open(file, 'w')
		f.write(self.__render())
		f.close()

	def __render(self):
		return """
<html>
<head>
	<title>%s</title>
</head>
<body>
	<!-- left side navigation -->
	<nav>
		%s
	</nav>
	<!-- main area -->
	<section>
		<h1>%s</h1>
		%s
	</section>
	<!-- footer -->
	<footer>
		%s
	</footer>
</body>
</html>
	""" % (self.title, self.nav, self.h1 or self.title, self.main, self.footer)


class NavigationNode:
	def __init__(self):
		self.EXPANDER = '<span class="js-expander">-</span>'
		self.__widget_member_filter_id = 0
		pass

	def nav_html(self):
		"""Returns the HTML for the left side navigation tree."""
		children_html = self.children_nav_html()
		node_html     = a(self.href(), self.text())
		if len(children_html):
			return """<li>%s %s
			<ol>
			%s
			</ol>
			</li>
			""" % (self.EXPANDER, node_html, children_html)
		else:
			return '<li class="leaf">%s</li>' % node_html

	def children_nav_html(self):
		"""Returns the children nodes' HTML for the left side navigation tree."""
		return "\n".join(child.nav_html() for child in self.children())

	def contents_html_template(self):
		"""Returns the HTML for the main content area."""
		return None

	def visit(self, f):
		"""Calls f for every node in the tree."""
		f(self)
		for child in self.children():
			child.visit(f)

	def href(self):
		"""Returns the file that this node should be written to."""
		raise NotImplementedError('You need to override href()')

	def text(self):
		"""Returns the title of this node in the tree."""
		raise NotImplementedError('You need to override text()')

	def children(self):
		"""Returns the child nodes of this node."""
		return []

	def is_content_empty(self):
		"""Checks if this node is empty. Used in combination with filter_empty."""
		return False

	def inherited_from(self, member_element):
		if not member_element.IsInherited:
			inherited_link = ""
		else:
			inherited_link = "(Inherited from %s.)" % member_element.DeclaringType.ToHtml()
		return inherited_link

	def ___access_css(self, element):
		return element.GetAccess().ToString().replace("PrivateScope, ", "")

	def __access_str(self, element):
		access = element.GetAccess()
		access_str = access.ToAccessString()
		access_str = """<span class="%s" title="%s">%s</span>""" % (access_str, access_str, access_str)

		if element.IsStatic:
			access_str += '<span class="static" title="static">static</span>'

		return access_str

	def properties_table(self, properties):
		"""Prints a table with the given properties."""

		def property_list_item(property_element):
			description = " ".join([
				property_element.XmlComment.Summary() or "&nbsp;",
				self.inherited_from(property_element)
			])
			tr_class    = " ".join([
				"inherited" if property_element.IsInherited else "",
				self.___access_css(property_element)
			])
			return """
				<tr class="%s">
					<td>%s</td>
					<td>%s</td>
					<td>%s</td>
					<td>%s</td>
				</tr>
			""" % (tr_class,
				   self.__access_str(property_element),
				   property_element.ToHtml(),
				   property_element.PropertyType.ToHtml(),
				   description)

		return fmt_non_empty(
			"""
			<table class="members properties">
				<thead>
					<tr>
						<th>&nbsp;</th>
						<th>Name</th>
						<th>Type</th>
						<th>Description</th>
					</tr>
				</thead>
				<tbody>
				%s
				</tbody>
			</table>""",
			"".join(property_list_item(p) for p in properties))

	def methods_table(self, methods):
		"""Prints a table with the given methods."""

		def method_list_item(method_element):
			description = " ".join([
				method_element.XmlComment.Summary() or "&nbsp;",
				self.inherited_from(method_element)
			])
			tr_class    = " ".join([
				"inherited" if method_element.IsInherited else "",
				self.___access_css(method_element)
			])
			result = """
				<tr class="%s">
					<td>%s</td>
					<td>%s</td>
					<td>%s</td>
				</tr>
			""" % (tr_class,
				   self.__access_str(method_element),
				   method_element.ToHtml(),
				   description)

			return result

		return fmt_non_empty(
			"""
			<table class="members methods">
				<thead>
					<tr>
						<th>&nbsp;</th>
						<th>Name</th>
						<th>Description</th>
					</tr>
				</thead>
				<tbody>
				%s
				</tbody>
			</table>""", "".join(method_list_item(m) for m in methods))

	def widget_member_filter(self):
		self.__widget_member_filter_id = self.__widget_member_filter_id + 1
		id_inherited = "chkShowInherited%s" % self.__widget_member_filter_id
		id_protected = "chkShowProtected%s" % self.__widget_member_filter_id
		return """

				<div>
					Show:
					<input type="checkbox" checked="checked" class="js-show-inherited" id="%s" />
					<label for="%s">Inherited</label>
					<input type="checkbox" checked="checked" class="js-show-protected" id="%s" />
					<label for="%s">Protected</label>
				</div>
			"""	% (id_inherited, id_inherited, id_protected, id_protected)


class NavigationDocumentationNode(NavigationNode):
	def __init__(self, documentation):
		NavigationNode.__init__(self)
		self.documentation = documentation

	def href(self):
		raise ValueError("You're not supposed to write the root node to disk.")

	def text(self):
		return None

	def children(self):
		return [ NavigationNamespaceNode(n) for n in self.documentation.Namespaces ]

	def nav_html(self):
		return "<ol>%s</ol>" % self.children_nav_html()

	def documentation(self):
		return self.documentation


class NavigationNamespaceNode(NavigationNode):
	def __init__(self, namespace_element):
		NavigationNode.__init__(self)
		self.namespace_element = namespace_element

	def href(self):
		return self.namespace_element.Filename()

	def text(self):
		return self.namespace_element.Namespace + " Namespace"

	def children(self):
		result = [ self.__type_node(t) for t in self.namespace_element.Types ]
		result.append(
			NavigationExtensionMethodsNode(self.namespace_element)
		)
		result = filter_empty(result)
		return result

	def contents_html_template(self):
		print "Generating page for namespace %s" % self.namespace_element.Namespace
		html_template       = HtmlTemplate()
		html_template.title = self.text()
		html_template.main  = "\n".join([
			self.__table("Classes", [t for t in self.namespace_element.Types if t.IsClass]),
			self.__table("Interfaces", [t for t in self.namespace_element.Types if t.IsInterface]),
			self.__table("Enumerations", [t for t in self.namespace_element.Types if t.IsEnum])
		])

		# TODO: delegates

		return html_template

	def documentation(self):
		return self.namespace_element.Documentation

	def __type_node(self, type_element):
		if type_element.IsEnum:
			return NavigationEnumNode(type_element)
		else:
			return NavigationTypeNode(type_element)

	def __table(self, title, types):
		def table_row(t):
			return """<tr>
				<td>%s</td>
				<td>%s</td>
			</tr>""" % (t.ToHtml(), t.XmlComment.Summary() or "&nbsp;")

		return fmt_non_empty("<h2>" + title + """</h2>
			<table>
				<thead>
					<tr>
						<th>Name</th>
						<th>Description</th>
					</tr>
				</thead>
				<tbody>
				%s
				</tbody>
			</table>
		""", "".join(table_row(t) for t in types))


class NavigationTypeNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.type_element.Filename()

	def text(self):
		return self.type_element.ToString("s") + " " + self.type_element.TypeKind

	def children(self):
		result = filter_empty([NavigationPropertiesNode(self.type_element), NavigationMethodsNode(self.type_element)])

		# TODO: constructors node
		# TODO: events node

		return result

	def contents_html_template(self):
		print "Generating page for type %s" % self.type_element.ToString("f")
		type_kind            = self.type_element.TypeKind
		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (self.type_element.ToString("f"), type_kind)
		html_template.h1     = "%s %s" % (self.type_element.ToString("s"), type_kind)
		html_template.main   = "\n".join([
			fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>""", self.type_element.XmlComment.Summary()),
			self.base_type_section(),
			self.interfaces_section(),
			self.derived_types_section(),
			self.__syntax_section(),
			self.constructors_section(),
			self.properties_section(),
			self.methods_section(),
			self.extension_methods_section()
		])

		# TODO: operators
		# TODO: separate attribute template?

		return html_template

	def __syntax_section(self):
		return '<h2>Syntax</h2><code class="syntax">%s</code>' % self.type_element.ToSyntax()

	def constructors_section(self):

		def constructor_list_item(constructor):
			return "<li>%s(%s)</li>" % ( self.type_element.ToString("s"), self.format_parameters(constructor) )

		return fmt_non_empty(
			"<h2>Constructors</h2><ul>%s</ul>",
			"".join(constructor_list_item(c) for c in self.type_element.Constructors))

	def base_type_section(self):
		base_type = self.type_element.BaseType

		# check if we're documenting System.Object or a class deriving directly from System.Object
		if not base_type or not base_type.BaseType:
			return ""

		return "<p>Inherits from %s</p>" % base_type.ToHtml()

	def interfaces_section(self):
		interfaces = self.type_element.GetInterfaces()
		if not interfaces:
			return ""

		return "<p>Implements interfaces: %s</p>" % ", ".join(t.ToHtml() for t in interfaces)

	def derived_types_section(self):
		derived_types = self.type_element.GetDerivedTypes()
		if not derived_types:
			return ""

		return "<p>Known derived types: %s</p>" % ", ".join(t.ToHtml() for t in derived_types)

	def properties_section(self):
		return fmt_non_empty(
			"<h2>Properties</h2>" + self.widget_member_filter() + "%s",
			self.properties_table(self.type_element.Properties))

	def methods_section(self):
		return fmt_non_empty(
			"<h2>Methods</h2>" + self.widget_member_filter() + "%s",
			self.methods_table(self.type_element.Methods))

	def extension_methods_section(self):
		return fmt_non_empty(
			"""
			<h2>Extension Methods</h2>
			%s
			""", self.methods_table(self.type_element.ExtensionMethods))

	def documentation(self):
		return self.type_element.Documentation

	def exclude_attribute(self, attribute):
		if attribute.AttributeType.Name == "__DynamicallyInvokableAttribute":
			return True

		return attribute.IsInstance(
			System.Runtime.CompilerServices.ExtensionAttribute,
			System.Runtime.TargetedPatchingOptOutAttribute,
			System.Security.SecuritySafeCriticalAttribute)

	def format_attribute(self, attribute):
		return "[" + attribute.AttributeType.ToHtml() + "]"

	def format_attributes(self, attributes):
		if not attributes:
			return ""

		return "".join(self.format_attribute(a) for a in attributes if not self.exclude_attribute(a))

	def format_parameter(self, parameterInfo):
		attributes = parameterInfo.GetCustomAttributes(False)
		result = self.format_attributes(attributes) + \
			parameterInfo.ParameterType.ToHtml() + \
			" " + parameterInfo.Name
		return result

	def format_parameters(self, something):
		print "format_parameters"
		result = ", ".join(self.format_parameter(p) for p in something.GetParameters())
		print "/format_parameters"
		return result


class NavigationEnumNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.type_element.Filename()

	def text(self):
		return self.type_element.ToString("s") + " " + self.type_element.TypeKind

	def contents_html_template(self):
		print "Generating page for enum %s" % self.type_element.ToString("f")
		type_kind            = self.type_element.TypeKind
		has_flags            = self.type_element.HasAttribute("System.FlagsAttribute")
		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (self.type_element.ToString("f"), type_kind)
		html_template.h1     = "%s %s" % (self.type_element.ToString("s"), type_kind)
		html_template.main   = fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>""", self.type_element.XmlComment.Summary())

		if has_flags:
			html_template.main += """
			<p class="info">This is a flags enum;
			its members can be combined with bitwise operators.</p>"""

		html_template.main += self.__members_section()

		return html_template

	def __members_section(self):
		def fmt_enum_member(enum_member):
			return "<tr><td>%s</td><td>%s</td><td>%s</td></tr>" % (
				enum_member.Name,
				enum_member.Value,
				enum_member.XmlComment.Summary() or "&nbsp;"
			)

		html = [ fmt_enum_member(enum_member) for enum_member in self.type_element.EnumMembers ]
		return """
		<h2>Members</h2>
		<table class=\"enum_members\">
			<thead>
				<tr>
					<th>Name</th>
					<th>Value</th>
					<th>Description</th>
				</tr>
			</thead>
			<tbody>
			%s
			</tbody>
		</table>
		""" % "".join(html)


class NavigationTypeMembersNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.type_element.Filename(prefix = self.text())

	def text(self):
		raise NotImplementedError("override me")

	def documentation(self):
		return self.type_element.Documentation

	def contents_html_template(self):
		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (self.type_element.ToString("f"), self.text())
		html_template.h1     = "%s %s" % (self.type_element.ToString("s"), self.text())
		html_template.main   = fmt_non_empty(
			self.widget_member_filter() + "%s",
			self.main_html_table())

		return html_template

	def is_content_empty(self):
		"""Checks if this node is empty. Used in combination with append_if_not_empty."""
		return len(self.children()) <= 0

	def main_html_table(self):
		raise NotImplementedError("override me")

class NavigationPropertiesNode(NavigationTypeMembersNode):
	def __init__(self, type_element):
		NavigationTypeMembersNode.__init__(self, type_element)

	def text(self):
		return "Properties"

	def children(self):
		return [ NavigationPropertyNode(p) for p in self.type_element.Properties if not p.IsInherited ]

	def main_html_table(self):
		return self.properties_table(self.type_element.Properties)

class NavigationMethodsNode(NavigationTypeMembersNode):
	def __init__(self, type_element):
		NavigationTypeMembersNode.__init__(self, type_element)

	def text(self):
		return "Methods"

	def children(self):
		return [ NavigationMethodNode(m) for m in self.type_element.Methods if not m.IsInherited ]

	def main_html_table(self):
		return self.methods_table(self.type_element.Methods)


class NavigationExtensionMethodsNode(NavigationNode):
	def __init__(self, namespace_element):
		NavigationNode.__init__(self)
		self.namespace_element = namespace_element

	def href(self):
		return self.namespace_element.Filename(prefix = "ExtensionMethods")

	def text(self):
		return "Extension Methods"

	def documentation(self):
		return self.namespace_element.Documentation

	def is_content_empty(self):
		return len(self.__get_extension_methods()) <= 0

	def contents_html_template(self):
		html_template        = HtmlTemplate()
		html_template.title  = "%s Extension Methods" % self.namespace_element.Namespace

		extension_methods = self.__get_extension_methods()
		methods_by_extended_type = { }
		for m in extension_methods:
			extended_type = m.GetParameters()[0].ParameterType
			lst = methods_by_extended_type.get(extended_type)
			if lst == None:
				lst = []
				methods_by_extended_type[extended_type] = lst

			lst.append(m)

		html_template.main = ""

		for extended_type in list(methods_by_extended_type):
			html_template.main += "<h2>Extension methods for %s</h2>" % extended_type.ToHtml()
			html_template.main += self.methods_table(methods_by_extended_type[extended_type])

		return html_template

	def __get_extension_methods(self):
		return [ m for m in self.namespace_element.Methods if not m.IsInherited and m.IsExtension() ]



class NavigationPropertyNode(NavigationNode):
	def __init__(self, property_element):
		NavigationNode.__init__(self)
		self.property_element = property_element

	def href(self):
		return self.property_element.Filename()

	def text(self):
		return self.property_element.Name

	def documentation(self):
		return self.property_element.Documentation

	def contents_html_template(self):
		property_name        = self.property_element.Name
		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (property_name, "Property")
		html_template.h1     = "%s.%s %s" % (self.property_element.OwnerType.ToString("s"), property_name, "Property")
		html_template.main   = fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>
				""", self.property_element.XmlComment.Summary()) + \
				"""
				<h2>Syntax</h2>
				<code class="syntax">
				%s
				</code>
				""" % self.property_element.ToSyntax()
		return html_template


class NavigationMethodNode(NavigationNode):
	def __init__(self, method_element):
		NavigationNode.__init__(self)
		self.method_element = method_element

	def href(self):
		return self.method_element.Filename()

	def text(self):
		return self.method_element.ToSignature()

	def documentation(self):
		return self.method_element.Documentation

	def contents_html_template(self):
		method_name          = self.method_element.Name
		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (method_name, "Method")
		html_template.h1     = "%s.%s %s" % (self.method_element.OwnerType.ToString("s"), method_name, "Method")
		html_template.main   = self.__summary_section() + \
			self.__syntax_section() + \
			self.__exceptions_section() + \
			self.__remarks_section() + \
			self.__example_section() + \
			self.__see_also_section()

		return html_template

	def __summary_section(self):
		return """
			<p>%s</p>
			<dl>
				<dt>Namespace</dt>
				<dd>%s</dd>
				<dt>Assembly</dt>
				<dd>%s</dd>
			</dl>
			""" % (self.method_element.XmlComment.Summary(),
				   self.method_element.NamespaceElement.ToHtml(),
				   self.method_element.OwnerType.Assembly.ToString())

	def __syntax_section(self):
		syntax = self.method_element.ToSyntax()

		result = """
			<h2>Syntax</h2>
			<code class="syntax">
			%s
			</code>
			""" % syntax

		result += self.__parameters_section()
		result += self.__return_value()
		return result

	def __parameter(self, parameter):
		return "<li>%s<br />Type: %s %s</li>" % (
			parameter.Name,
			parameter.ParameterType.ToHtml(),
			self.method_element.XmlComment.Param(parameter.Name)
		)

	def __parameters_section(self):
		return fmt_non_empty("""
			<h3>Parameters</h3>
			<ol>
			%s
			</ol>
			""", "".join(self.__parameter(p) for p in self.method_element.GetParameters()))

	def __return_value(self):
		return """
		<h3>Return Value</h3>
		<p>Type: %s</p>
		<p>%s</p>
		""" % (self.method_element.ReturnType.ToHtml(), self.method_element.XmlComment.Section("returns"))

	def __exceptions_section(self):
		return ""

	def __remarks_section(self):
		return self.method_element.XmlComment.Section("remarks")

	def __example_section(self):
		return self.method_element.XmlComment.Section("example")

	def __see_also_section(self):
		return ""


def make_visitor(nav, footer):
	"""Makes the visitor function that processes each navigation node."""

	def visitor(navigation_node):
		print "visting %s" % navigation_node
		html_template = navigation_node.contents_html_template()
		if not html_template:
			return

		html_template.nav    = nav
		html_template.footer = footer
		html_template.write(navigation_node.href())

	return visitor

#	def generate_nant_task_pages(self):
#		print "NAnt tasks:"
#		for type in self.documentation.Types:
#			if type.HasAttribute('NAnt.Core.Attributes.TaskName'):
#				taskName = type.GetAttribute('NAnt.Core.Attributes.TaskName').Name
#				print "todo: generate page for nant task " + taskName

def Generate(documentation):
	"""Entry point for IglooCastle"""
	print "Hello from python!"

	root_nav_node = NavigationDocumentationNode(documentation)
	nav           = root_nav_node.nav_html()
	footer        = """Generated by IglooCastle at
			""" + strftime("%Y-%m-%d %H:%M:%S", gmtime()) + """
			<link type=\"text/css\" rel=\"stylesheet\" href=\"style.css\" />
			<script src="jquery-1.11.1.min.js"></script>
			<script src="app.js"></script>
			</footer>"""
	visitor = make_visitor(nav, footer)
	root_nav_node.visit(visitor)
	print "Python out!"
