# IRIS-Viz Meta Quest 3
This is the IRIS Unity App for the Meta Quest 3.


## Option 1: Install the pre-built APK
If there is no need to modify the code or if you simply want to test the functionality of the APP, you can directly use our released version.

Download the APK from the latest release onto your device and install it following [our documentation](https://intuitive-robots.github.io/iris-project-page/xr_application/meta_quest3.html). You will find it in the Library under "Unknown Sources". If your device is already in developer mode you can also use the android debugger to install the apk via USB from your PC with `adb install IRIS-Meta-Quest3.apk`.

## Option 2: Build from source
If you need to modify the unity code, you will have to build the APK from source. For this it is recommended to put your quest into developer mode. See [this guide](https://github.com/rail-berkeley/oculus_reader?tab=readme-ov-file#setup-of-the-adb) for putting the quest into developer mode. Then follow these steps:

1. Clone this repo and download and install the Unity Hub
2. Open this repo in the Unity Hub, it should already suggest you the correct unity version (6000.0.24f1), also tick the android build support
3. Open the project and go to Window > Package Manager and update all Meta related packages to version 81.0.0
4. Ppen the IRIS scene: File > Open Scences > select the IRIS scene under the folder `Scenes`
5. Switch the build platform to android: File > Build Profiles > Android > Switch Platform
6. Switch the build profile to IRIS: File > Build Profiles > Build Profiles > iris meta quest 3 > override global scene list
7. Compile: File > Build Profiles > Build Profiles > iris meta quest 3 > Build then choose a name for your apk and confirm to pop

## Resources
*   [IRIS-Viz Meta Quest 3 Documentation](https://intuitive-robots.github.io/iris-project-page/xr_development/iris_viz.html)
*   [Main Python Client Repository](https://github.com/intuitive-robots/SimPublisher)

