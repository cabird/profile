import re, os, glob

def main():
    pattern = "\s*@.*\s*{\s*([-a-zA-Z0-9_]+)\s*,"
    keys = []
    for line in open("bird.bib").readlines():
        if not "@" in line:
            continue
        match = re.match(pattern, line)
        if match:
            keys.append(match[1])
        else:
            #this is a check to make sure we don't miss anything
            print(line)
    keys.sort()
    print(keys)
    print(f"There are {len(keys)} publications")

    pdfs = [os.path.basename(path).split(".")[0] for path in glob.glob("pubs/*.pdf")]
    pdfs.sort()

    for key in keys:
        if not key in pdfs:
            print(f"Could not find pdf for {key}")
    for pdf in pdfs:
        if not pdf in keys:
            print(f"pdf {pdf} has no key in bibtex")

        


if __name__ == "__main__":
    main()