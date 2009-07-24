#! /usr/bin/ruby

require 'set'

#we assume no bibtex entry is over this size... set it big enough that seems sane.
BLOCKSIZE = 4096

#for some fields, we'd like to specify their order.  This defines that order
FieldOrder = %w(author title booktitle edition journal volume number pages publisher month year publisher location organization)

class Entry
	attr_reader :type, :key, :fields
	def initialize(type, key)
		@type = type
		@key = key
		@fields = {}
	end

	def setField(name, value)
		@fields[name] = value
	end
	
	# return a string that is a valid bibtex entry
	def to_s 
		s = "@#{@type}{#{@key},\n"
		keys = @fields.keys
		#first add the ordered fields in order if they exist
		FieldOrder.each do |field|
			if keys.include? field
				s += "  #{field.capitalize} = #{@fields[field]},\n"
				keys.delete field
			end
		end
		#now add all of the others
		keys.each { |key| s += "  #{key.capitalize} = #{@fields[key]},\n"}
		s += "}"
	end

	def toPreBibtex
		s = "@#{@type}{#{@key},\n"
		#first add the ordered fields in order if they exist
		keys = @fields.keys
		FieldOrder.each do |field|
			if keys.include? field
				s += " " * 4 + "#{field.capitalize} = #{@fields[field]},\n"
				keys.delete field
			end
		end
		#now add all of the others
		s += "}\n"
	end

	def toHTML
		cfields = {}
		@fields.each{ |k, v| cfields[k] = v.gsub(/[\}\{]/, "").strip }
		s = cfields["author"].split(" and ").map{|x| x.strip}.join(", ") + ". "
		s += cfields["title"] + ". "
		s += "In " + cfields["booktitle"]  if cfields.key? "booktitle"
		s += "In " + cfields["journal"]  if cfields.key? "journal"
		s += " (" + cfields["abbrv"] + ")" if cfields.key? "abbrv"
		s += ", " + cfields["location"] if cfields.key? "location"
		if cfields.key? "month"
			s += ", " + cfields["month"] + " " + cfields["year"]
		else
			s += ", " + cfields["year"]
		end
		s += "."

	end
end


# parse a file and return an ordered list of keys and hash mapping cite keys to bibtex entrie
def parseFile(file)
	#TODO handle @comment and @string
	#print "parsing " + file	
	$stdout.flush
	bibFile = open(file)
	text = bibFile.read(BLOCKSIZE)
	#keep track of the order of the keys so we keep things in order
	#when we print them back out
	keys = []
	entries = {}
	#get the bibtex entry type and the cite key
	while text =~ /@(\w+)/ do
		if $1.downcase == "comment" 
			puts "comment"
			text = $'
		elsif $1.downcase == "string" 
			puts "string"
			text = $'
		elsif text =~ /@(\w+)\s*(\{|\()\s*([^,\s]*)/
			type = $1
			entryDelim = $2
			key = $3
			entry = Entry.new(type, key)
			entries[key] = entry
			keys << key
			#puts "*** Entry #{key} is of type #{type} ***"  
			text = $'
			#parse out all of the fields
			#this grabs the field name and equals sign
			while text =~ /\A\s*,\s*(\S+)\s*\=\s*/ do
				#normalize field names
				fieldName = $1.downcase
				text = $'
				field, text = parseField text	
				entry.setField(fieldName, field)
			end
		else
			#should throw exception and print to stderr
			puts "bad formatted entry", $1
		end
		#puts entry.to_s
		# read some more from the file if we're getting low
		text += bibFile.read(2*BLOCKSIZE) if text.size < BLOCKSIZE and !bibFile.eof?
	end

	#print " #{keys.length} entries\n"

	return keys, entries
end

# the rules for parsing the value for a field are super weird... see:
# http://artis.imag.fr/~Xavier.Decoret/resources/xdkbibtex/bibtex_summary.html
# long time ruby users will probably cry if they see this...
def parseField(text)
	#if there is no delimiter, then just grab the next block of alphanumerics
	if text =~ /\A\s*(\w+)/ then return $1, $' end
	#figure out what the delimiter is.  Either { or "
	i = text =~ /\A\s*(\"|\{)/
	if !i then print "text has bad delimiter " + text[0..20]; exit(1) end
	delim = $1
	i = text.index(delim)
	start = i
	field = ""
	#the rules are different for if the delimiter is a { or a "
	if delim == "{" then
		nesting = 1
		loop do
			i += 1
			nesting += 1 if text[i].chr == "{"
			nesting -= 1 if text[i].chr == "}"
			#return the field (including delimiter) and the rest of the text
			return text[start..i], text[i+1..-1] if nesting == 0
		end
	end
	if delim == '"' then
		nesting = 0
		loop do
			i += 1
			nesting += 1 if text[i].chr == "{"
			nesting -= 1 if text[i].chr == "}"
			return text[start..i], text[i+1..-1] if nesting == 0 and text[i].chr == "\""
			if nesting > 20 then 1/0 end
		end
	end
end

# order of files actually matters.  In this case, the ordering of keys in 
# file1 is preserved over the ordering in file2
def merge(file1, file2, file3)
	keys1, entries1 = parseFile file1
	keys2, entries2 = parseFile file2
	out = open(file3, "w")

	newKeys = keys2 - keys1
	print "there are #{newKeys.length} in #{file2} that are not in #{file1}\n"
	puts newKeys
	entries1.update entries2
	(keys1 + newKeys).each { |key| out << entries1[key] << "\n\n" }
end

def shownew(file1, file2)
	keys1, entries1 = parseFile file1
	keys2, entries2 = parseFile file2
	newKeys = keys2 - keys1
	print "there are #{newKeys.length} in #{file2} that are not in #{file1}\n"
	puts newKeys
end

def makePage(bibFile, htmlFile)
	keys, entries = parseFile bibFile
	template = open("template.html").read()
	clickFuncs = []
	bibLines = []
	keys.each_with_index do |key, index|
		entry = entries[key]
		clickFuncs << "$('#show_#{key}').click(function(){$('#hidden_#{key}').show('slow')});"
		clickFuncs << "$('#hide_#{key}').click(function(){$('#hidden_#{key}').hide('slow')});"

		pdfLink = ""
		if entry.fields.key? "pdf"
			pdf = entry.fields["pdf"]
			pdf.gsub!(/[\}\{]/, "")	
			pdfLink = "<a href='papers/#{pdf}'>PDF</a>"
		end


		absCode = ""
		absLink = ""

		if entry.fields.key? "abstract"
			clickFuncs << "$('#show_#{key}_abstract').click(function(){$('#hidden_#{key}_abstract').show('slow')});"
			clickFuncs << "$('#hide_#{key}_abstract').click(function(){$('#hidden_#{key}_abstract').hide('slow')});"
			absText = entry.fields["abstract"].gsub(/[\{\}]/, "").strip 
			absText.gsub!(/\n\n/, "<br><br>")	
			absLink = "<a id='show_#{key}_abstract' href='javascript:'>show abstract</a>"
			absCode = <<EOF
<div id="hidden_#{key}_abstract" class="initial_hidden">
	<p class="abstract">#{absText}</p>
	<div id="hide_#{key}_abstract"><a href="javascript:">Hide</a></div>
</div>
EOF
		end


		bibLines << <<EOF
<p>#{entry.toHTML}<br>
#{pdfLink}
#{absLink}
<a id="show_#{key}" href="javascript:">show bibtex</a>
#{absCode}
<div id="hidden_#{key}" class="initial_hidden">
	<pre class="bibtex">#{entry.toPreBibtex}</pre>
	<div id="hide_#{key}"><a href="javascript:">Hide</a></div>
</div>
EOF

	end
	template.sub!(/\/\*CLICK FUNCS\*\//, clickFuncs.join("\n" + " " * 8))	
	template.sub!(/<!--BIBS-->/, bibLines.join("\n"))
	print template

end

#I wish I knew the if __name__ == "__main__": equivalent in ruby

case ARGV[0]
	when "merge": merge(ARGV[1], ARGV[2], ARGV[3])
	when "parse": parseFile(ARGV[1])[1].each { |key, entry| puts entry }
	when "count": parseFile(ARGV[1])
	when "shownew": shownew(ARGV[1], ARGV[2])
	when "makepage" : makePage(ARGV[1], ARGV[2])
end
