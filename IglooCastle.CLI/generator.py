import sys
import clr
import System
from time import gmtime, strftime

clr.AddReference("IglooCastle.CLI")
import IglooCastle.CLI
clr.ImportExtensions(IglooCastle.CLI)
from IglooCastle.CLI import FilenameProvider

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

class HtmlTemplate:
	def __init__(self):
		self.title  = ""
		self.h1     = ""
		self.nav    = ""
		self.main   = ""
		self.footer = ""

	def render(self):
		return """<html>
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

	def write_to(self, file):
		print "Writing file %s" % file
		f = open(file, 'w')
		f.write(self.render())
		f.close()

	@staticmethod
	def fmt_non_empty(template, contents):
		"""Formats the contents into template, if the contents are not empty."""
		if len(contents):
			return template % contents
		else:
			return ""


class NavigationNode:
	def __init__(self):
		self.filename_provider = FilenameProvider()
		self.EXPANDER = '<span class="js-expander">-</span>'
		pass

	def nav_html(self):
		children_html = self.children_nav_html()
		node_html     = a(self.href(), self.text())
		if len(children_html):
			return "<li>%s %s<ol>%s</ol></li>" % (self.EXPANDER, node_html, children_html)
		else:
			return '<li class="leaf">%s</li>' % node_html

	def contents_html_template(self):
		return None

	def visit(self, f):
		"""Calls f for every node in the tree."""
		f(self)
		for child in self.children():
			child.visit(f)

	def href(self):
		raise ValueError('You need to override href()')

	def text(self):
		raise ValueError('You need to override text()')

	def children(self):
		return []

	def children_nav_html(self):
		return "".join(child.nav_html() for child in self.children())

	def property_link(self, property_element):
		t = property_element.DeclaringType
		if not property_element.Documentation.IsLocalType(t):
			return property_element.Name
		else:
			return a(self.filename_provider.Filename(property_element), property_element.Name)

	def method_link(self, method_element):
		t = method_element.DeclaringType
		signature = self.type_printer().Print(method_element)
		if not method_element.Documentation.IsLocalType(t):
			return signature
		else:
			return a(self.filename_provider.Filename(method_element), signature)

	def type_printer(self):
		return self.documentation().TypePrinter


class NavigationDocumentationNode(NavigationNode):
	def __init__(self, documentation):
		NavigationNode.__init__(self)
		self.documentation = documentation

	def href(self):
		return None

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
		return self.filename_provider.Filename(self.namespace_element)

	def text(self):
		return self.namespace_element.Namespace + " Namespace"

	def children(self):
		result = [ self.__type_node(t) for t in self.namespace_element.Types ]
		result.append( NavigationExtensionMethodsNode(self.namespace_element) )
		return result

	def contents_html_template(self):
		print "Generating page for namespace %s" % self.namespace_element.Namespace
		html_template = HtmlTemplate()
		html_template.title = self.text()
		html_template.main  = \
			HtmlTemplate.fmt_non_empty("""
				<h2>Classes</h2>
				<ol>
					%s
				</ol>
				""", "".join("<li>" + self.type_printer().Print(t) + "</li>" for t in self.namespace_element.Types if t.IsClass)) + \
			HtmlTemplate.fmt_non_empty("""
				<h2>Interfaces</h2>
				<ol>
					%s
				</ol>
				""", "".join("<li>" + self.type_printer().Print(t) + "</li>" for t in self.namespace_element.Types if t.IsInterface)) + \
			HtmlTemplate.fmt_non_empty("""
				<h2>Enumerations</h2>
				<ol>
					%s
				</ol>
				""", "".join("<li>" + self.type_printer().Print(t) + "</li>" for t in self.namespace_element.Types if t.IsEnum))

		# TODO: delegates

		return html_template

	def documentation(self):
		return self.namespace_element.Documentation

	def __type_node(self, type_element):
		if type_element.IsEnum:
			return NavigationEnumNode(type_element)
		else:
			return NavigationTypeNode(type_element)


class NavigationTypeNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.filename_provider.Filename(self.type_element)

	def text(self):
		return self.type_element.ShortName + " " + self.type_element.TypeKind

	def children(self):
		result = []

		# TODO: constructors node

		n = NavigationPropertiesNode(self.type_element)
		if len(n.children()):
			result.append(n)

		n = NavigationMethodsNode(self.type_element)
		if len(n.children()):
			result.append(n)

		# TODO: events node

		return result

	def contents_html_template(self):
		print "Generating page for type %s" % self.type_element.FullName
		type_kind            = self.type_element.TypeKind
		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (self.type_element.FullName, type_kind)
		html_template.h1     = "%s %s" % (self.type_element.ShortName, type_kind)
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>""", self.type_element.XmlComment.Summary()) + \
			self.base_type_section() + \
			self.interfaces_section() + \
			self.derived_types_section() + \
			self.constructors_section() + \
			self.properties_section() + \
			self.methods_section() + \
			self.extension_methods_section()

		# TODO: operators
		# TODO: separate attribute template?

		return html_template

	def constructors_section(self):

		def constructor_list_item(constructor):
			return "<li>%s(%s)</li>" % ( self.type_element.ShortName, self.format_parameters(constructor) )

		return HtmlTemplate.fmt_non_empty(
			"<h2>Constructors</h2><ul>%s</ul>",
			"".join(constructor_list_item(c) for c in self.type_element.Constructors))

	def base_type_section(self):
		base_type = self.type_element.BaseType
		if not base_type or base_type.FullName == "System.Object":
			return ""

		return "<p>Inherits from %s</p>" % self.type_printer().Print(base_type)

	def interfaces_section(self):
		interfaces = self.type_element.GetInterfaces()
		if not interfaces:
			return ""

		return "<p>Implements interfaces: %s</p>" % ", ".join(self.type_printer().Print(t) for t in interfaces)

	def derived_types_section(self):
		derived_types = self.type_element.GetDerivedTypes()
		if not derived_types:
			return ""

		return "<p>Known derived types: %s</p>" % ", ".join(self.type_printer().Print(t) for t in derived_types)

	def inherited_from(self, member_element):
		if not member_element.IsInherited:
			inherited_link = ""
		else:
			inherited_link = "(inherited from %s)" % self.type_printer().Print(member_element.DeclaringType)
		return inherited_link

	def properties_section(self):
		def property_list_item(property):
			name        = property.Name
			ptype       = self.type_printer().Print(property.PropertyType)
			description = property.XmlComment.Summary() + " " + self.inherited_from(property)
			link        = self.filename_provider.Filename(property)
			return """<tr>
			<td>%s</td>
			<td>%s</td>
			<td>%s</td>
			</tr>""" % (self.property_link(property), ptype, description)

		return HtmlTemplate.fmt_non_empty(
			"""<h2>Properties</h2>
			<table>
				<thead>
				<tr>
					<th>Name</th>
					<th>Type</th>
					<th>Description</th>
				</tr>
				</thead>
				<tbody>
				%s
				</tbody>
			</table>""",
			"".join(property_list_item(p) for p in self.type_element.Properties))

	def methods_section(self):
		def method_list_item(method_element):
			inheritedLink = self.inherited_from(method_element)
			result = """<li>
				<dl>
					<dt>%s</dt>
					<dd>%s %s</dd>
				</dl>
			</li>""" % (self.method_link(method_element), method_element.XmlComment.Summary(), inheritedLink)

			return result

		declared_methods  = [ m for m in self.type_element.Methods if not m.IsInherited ]
		inherited_methods = [ m for m in self.type_element.Methods if m.IsInherited ]

		declared_methods_html = HtmlTemplate.fmt_non_empty(
			"<h3>Declared in this type</h3><ol>%s</ol>",
			"".join(method_list_item(m) for m in declared_methods))

		inherited_methods_html = HtmlTemplate.fmt_non_empty(
			'<h3>Inherited</h3><ol class="inherited">%s</ol>',
			"".join("<li> " + self.method_link(m) + "</li>" for m in inherited_methods))

		return HtmlTemplate.fmt_non_empty(
			"<h2>Methods</h2>%s", declared_methods_html + inherited_methods_html)

	def extension_methods_section(self):
		return "<h2>Extension methods</h2><p>todo methods that extend this class defined in this documentation</p>"

	def documentation(self):
		return self.type_element.Documentation

	def exclude_attribute(self, attribute):
		if attribute.GetType().Name == "__DynamicallyInvokableAttribute":
			return True

		return isinstance(attribute, (
			System.Runtime.CompilerServices.ExtensionAttribute,
			System.Runtime.TargetedPatchingOptOutAttribute,
			System.Security.SecuritySafeCriticalAttribute))

	def format_attribute(self, attribute):
		return "[" + self.type_printer().Print(attribute.GetType()) + "]"

	def format_attributes(self, attributes):
		if not attributes:
			return ""

		return "".join(self.format_attribute(a) for a in attributes if not self.exclude_attribute(a))

	def format_parameter(self, parameterInfo):
		attributes = parameterInfo.GetCustomAttributes(False)

		return self.format_attributes(attributes) + \
			self.type_printer().Print(parameterInfo.ParameterType) + \
			" " + parameterInfo.Name

	def format_parameters(self, something):
		return ", ".join(self.format_parameter(p) for p in something.GetParameters())


class NavigationEnumNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.filename_provider.Filename(self.type_element)

	def text(self):
		return self.type_element.ShortName + " " + self.type_element.TypeKind

	def contents_html_template(self):
		print "Generating page for enum %s" % self.type_element.FullName
		type_kind            = self.type_element.TypeKind
		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (self.type_element.FullName, type_kind)
		html_template.h1     = "%s %s" % (self.type_element.ShortName, type_kind)
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>""", self.type_element.XmlComment.Summary()) + \
			self.__members_section()

		return html_template

	def __members_section(self):
		names = System.Enum.GetNames(self.type_element.Member)

		html = [ "<tr><td>%s</td><td>%s</td><td>%s</td></tr>" % ( name, "", "" ) for name in names ]
		return """
		<h2>Members</h2>
		<table>
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


class NavigationPropertiesNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.filename_provider.Filename(self.type_element, "Properties")

	def text(self):
		return "Properties"

	def children(self):
		return [ NavigationPropertyNode(p) for p in self.type_element.Properties if not p.IsInherited ]

	def documentation(self):
		return self.type_element.Documentation

	def contents_html_template(self):
		html_template        = HtmlTemplate()
		html_template.title  = "%s Properties" % self.type_element.FullName
		html_template.h1     = "%s Properties" % self.type_element.ShortName
		html_template.main   = "<ol>%s</ol>" % "".join("<li>" + self.property_link(p) + "</li>" for p in self.type_element.Properties)
		return html_template


class NavigationMethodsNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.filename_provider.Filename(self.type_element, "Methods")

	def text(self):
		return "Methods"

	def children(self):
		return [ NavigationMethodNode(m) for m in self.type_element.Methods if not m.IsInherited ]

	def documentation(self):
		return self.type_element.Documentation

	def contents_html_template(self):
		html_template        = HtmlTemplate()
		html_template.title  = "%s Methods" % self.type_element.FullName
		html_template.h1     = "%s Methods" % self.type_element.ShortName
		html_template.main   = "<ol>%s</ol>" % "".join("<li>" + self.method_link(m) + "</li>" for m in self.type_element.Methods)
		return html_template


class NavigationExtensionMethodsNode(NavigationNode):
	def __init__(self, namespace_element):
		NavigationNode.__init__(self)
		self.namespace_element = namespace_element

	def href(self):
		return self.filename_provider.Filename(self.namespace_element, "ExtensionMethods")

	def text(self):
		return "Extension Methods"

	def documentation(self):
		return self.namespace_element.Documentation

	def contents_html_template(self):
		html_template        = HtmlTemplate()
		html_template.title  = "%s Extension Methods" % self.namespace_element.Namespace

		# todo: group per target type
		extension_methods = [ m for m in self.namespace_element.Methods if not m.IsInherited and m.IsExtension() ]
		dct = { }
		for m in extension_methods:
			extended_type = m.GetParameters()[0].ParameterType
			lst = dct.get(extended_type)
			if lst == None:
				lst = []
				dct[extended_type] = lst

			lst.append(m)

		html_template.main = ""

		for t in list(dct):
			html_template.main += "<h2>%s</h2>" % self.type_printer().Print(t)
			html_template.main += "<ol>"
			html_template.main += "".join("<li>" + self.method_link(m) + "</li>" for m in dct[t])
			html_template.main += "</ol>"

		return html_template


class NavigationPropertyNode(NavigationNode):
	def __init__(self, property_element):
		NavigationNode.__init__(self)
		self.property_element = property_element

	def href(self):
		return self.filename_provider.Filename(self.property_element)

	def text(self):
		return self.property_element.Name

	def documentation(self):
		return self.property_element.Documentation

	def contents_html_template(self):
		property_name = self.property_element.Name
		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (property_name, "Property")
		html_template.h1     = "%s.%s %s" % (self.property_element.OwnerType.ShortName, property_name, "Property")
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>
				""", self.property_element.XmlComment.Summary()) + \
				"""
				<h2>Syntax</h2>
				<code>
				%s
				</code>
				""" % self.type_printer().Syntax(self.property_element)
		return html_template


class NavigationMethodNode(NavigationNode):
	def __init__(self, method_element):
		NavigationNode.__init__(self)
		self.method_element = method_element

	def href(self):
		return self.filename_provider.Filename(self.method_element)

	def text(self):
		return self.type_printer().Print(self.method_element)

	def documentation(self):
		return self.method_element.Documentation

	def contents_html_template(self):
		method_name = self.method_element.Name
		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (method_name, "Method")
		html_template.h1     = "%s.%s %s" % (self.method_element.OwnerType.ShortName, method_name, "Method")
		html_template.main   = self.__summary_section() + \
			self.__syntax_section() + \
			self.__exceptions_section() + \
			self.__remarks_section() + \
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
			""" % (self.method_element.XmlComment.Summary(), self.type_printer().Print(self.method_element.NamespaceElement), self.method_element.OwnerType.Assembly.ToString())

	def __syntax_section(self):
		syntax = self.type_printer().Syntax(self.method_element)

		result = """
			<h2>Syntax</h2>
			<code>
			%s
			</code>
			""" % syntax

		result += self.__parameters_section()
		result += self.__return_value()
		return result

	def __parameter(self, parameter):
		return """<li>
		%s
		<br />
		Type: %s
		%s
		</li>""" % (parameter.Name, self.type_printer().Print(parameter.ParameterType), self.method_element.XmlComment.Param(parameter.Name))

	def __parameters_section(self):
		return HtmlTemplate.fmt_non_empty("""
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
		""" % (self.type_printer().Print(self.method_element.ReturnType), self.method_element.XmlComment.Section("returns"))

	def __exceptions_section(self):
		return ""

	def __remarks_section(self):
		return ""

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
		html_template.write_to(navigation_node.href())

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
