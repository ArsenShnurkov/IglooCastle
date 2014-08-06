import sys

def Generate(documentation):
	print "Hello from python!"
	print "Namespaces:"
	for namespace in documentation.Namespaces:
		print namespace

	print "Types:"
	for t in documentation.Types:
		print t

