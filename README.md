# UnityFaceApi
This repository contains a sample of face identification using Microsoft Azure Face API.

## Structure
### Assets
You can find some images of me for training under Assets/App/Resources/Train. There are also some images under Assets/App/Resources/Test.
### Scene
The scene Main.scene under Assets/App/Scenes is the main sample scene. The FaceRecognizer object and script is the only object of interest in this project.
The project follows face identification steps outlined at the [Microsoft tutorial page](https://docs.microsoft.com/en-us/azure/cognitive-services/face/face-api-how-to-topics/howtoidentifyfacesinimage).
* Checks if the specified PersonGroup exists, if not create one then stop.
* If the PersonGroup exists, check if there are any Person objects, if not, create one then stop.
* If a PersonGroup exists an it has at least one Person object, check if it is already trained. If not, train using the images in the Assets/App/Resources/Train and stop if successful.
* If all the above checks return true, then identify faces in the test images located at Assets/App/Resources/Test.
The detection results are returned as JSON and currently there are no visualization features in the project.
One way to visualize the result is use another library such as OpenCV and feed the detected Rect in the JSON.
### Scripts
Assets/App/Scripts contain all the scripts for this project. FaceRecognizer.cs is the main script, but the wrapper methods for the HTTP calls to the Face API can be found in RequestManager.cs. Other scripts are just JSON definitions to retrieve the various responses from the Face API.

### Unity Packman
This repository uses [Unity Packman](https://www.npmjs.com/package/unity-packman) to enable other repositories to import the scripts in this project. Currently the scripts under Assets/Scripts/Common will be exported.
