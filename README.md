# KSCC
Calibration Kinect skeleton positions of two players for following motion comparision

1. Prerequistites

* Win7 or later
* Kinect SDK 1.7
* Visual Studio 2012 or later
* Kinect for Windows V1

2. MotionAnalysisTeacher

connect one Kinect
run the server app
setup the TCP server

3. MotionAnalysisStudent

connect another Kinect to the same PC (might not be detected sometimes)
run the client app
connect the TCP server

4. Calibration (detail algorithm in the attached paper)

two players face their Kinects respectively.
keep same gesture for around 4 seconds (120 frames data)

5. Motion Comaprison

After calibration, the motions of the two players will be comparied. 

##Node:
the current version just a prototype for research purpose. It could be separated into two or more remote systems.