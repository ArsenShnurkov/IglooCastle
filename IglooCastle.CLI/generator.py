import sys

#
# linking
#

def isLocalType(documentation, t):
	# TODO: not-optimal
	for typeElement in documentation.Types:
		if typeElement.Type == t:
			return True

	return False

def typeLink(documentation, t):
	if isLocalType(documentation, t):
		link = t.FullName + ".html"
		return "<a href=\"" + link + "\">" + t.FullName + "</a>"
	else:
		return t.FullName

#
# core
#

def typeProperties(documentation, typeElement):
	result = ""
	for p in typeElement.Properties:
		result = result + "<li>"
		result = result + p.Property.Name
		result = result + ": "
		result = result + p.XmlComment.Summary
		if p.Property.DeclaringType != typeElement.Type:
			result = result + " (inherited from " + typeLink(documentation, p.Property.DeclaringType) + ")"
		result = result + "</li>"
	if len(result):
		result = "<h2>Properties</h2><ul>" + result + "</ul>"

	return result


def typeMethods(documentation, typeElement):
	result = ""
	for p in typeElement.Methods:
		result = result + "<li>"
		result = result + p.Method.Name
		if p.Method.DeclaringType != typeElement.Type:
			result = result + " (inherited from " + typeLink(documentation, p.Method.DeclaringType) + ")"
		result = result + "</li>"
	if len(result):
		result = "<h2>Methods</h2><ul>" + result + "</ul>"

	return result

def nav(documentation):
	result = ""
	for t in documentation.Types:
		fullName = t.Type.FullName
		link = fullName + '.html'
		result = result + "<li><a href=\"" + link + "\">" + t.Type.FullName + "</a></li>"
	if len(result):
		result = "<nav><ul>" + result + "</ul></nav>"

	return result

def Generate(documentation):
	print "Hello from python!"

	header = nav(documentation)
	footer = "<footer><link type=\"text/css\" rel=\"stylesheet\" href=\"style.css\" /></footer>"

	print "Namespaces:"
	for namespace in documentation.Namespaces:
		print namespace
		f = open(namespace + '.html', 'w')
		f.write("""<html>
			<head>
				<title>""" + namespace + """</title>
			</head>
			<body>
				<h1>Namespace """ + namespace + """</h1>""" + header + """
				""" + footer + """
			</body>
		</html>
""")
		f.close()



	print "Types:"
	for typeElement in documentation.Types:
		fullName = typeElement.Type.FullName
		print fullName
		f = open(fullName + '.html', 'w')
		f.write("""<html>
			<head>
				<title>""" + fullName + """</title>
			</head>
			<body>
				<h1>Type """ + fullName + """</h1>""" +	header + """
				<p>""" + typeElement.XmlComment.Summary + """</p>""" +
				typeProperties(documentation, typeElement) +
				typeMethods(documentation, typeElement) + """
				""" + footer + """
			</body>
		</html>
""")
		f.close()

