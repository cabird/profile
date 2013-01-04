import urllib2, json, sys, os, time

#holds all info needed regarding a repository that needs to be autobuilt
class Repo:
	def __init__(self, user, repo, branch, localPath):
		self.user = user
		self.repo = repo
		self.branch = branch
		self.localPath = localPath

	def url(self):
		return "https://api.github.com/repos/%s/%s/events" % (self.user, self.repo)

class EventsResponse:
	def __init__(self, message, etag):
		self.message = message
		self.etag = etag

def main():
	etag = None
	#todo - should have this in a config file.  Should support multiple repos.
	repo = Repo("cabird", "cv", "master", "/home/cbird/Documents/GitHub/cv")
	delaySeconds = 5

	while True:
		print "requesting with etag", etag
		# don't fail on an exception.  If there is a problem, then just
		# continue on (should probably log it).
		try:
			eventsResponse = RequestEvents(repo, etag)

			if eventsResponse.message:
				print "got message:"
				print eventsResponse.message
				if not "AUTOBUILT" in eventsResponse.message:
					BuildAndCommit(repo)
			etag = eventsResponse.etag
		except Exception e:
			print e

		time.sleep(delaySeconds)

def BuildAndCommit(repo):
	#TODO - this assumes all tools are on the path
	#TODO - need to add checking and dealing with errors

	print "building"
	os.chdir(repo.localPath)
	os.system("git reset --hard")
	os.system("git pull")
	os.system("make clean")
	os.system("make > make.log")
	os.system("git add make.log")
	os.system("git commit -am \"AUTOBUILT\"")
	os.system("git push")


def RequestEvents(repo, etag = None):
	req = urllib2.Request(repo.url())
	if etag:
		req.add_header("If-None-Match", etag)

	#try to get a response, if no new events are available, then
	#the response will be 304
	try:
		resp = urllib2.urlopen(req)
	except urllib2.HTTPError, e:
		print e.code
		if e.code == 304:
			#there are no new events
			print "no new events"
			return EventsResponse(None, etag)
		raise e

	info = resp.info()
	etag = info.getheader("etag")
	print etag
	
	# get first push Event
	jsonData = json.loads(resp.read())
	pushEvent = None
	for event in jsonData:
		# get only push events
		if event["type"] == "PushEvent":
			# only match pushes to the branch we care about
			if event["payload"]["ref"] == "refs/heads/" + repo.branch:
				pushEvent = event
				break
	else:
		return EventsResponse(None, etag)

	message = pushEvent["payload"]["commits"][0]["message"]
	return EventsResponse(message, etag)
		
if __name__ == "__main__":
	main()
