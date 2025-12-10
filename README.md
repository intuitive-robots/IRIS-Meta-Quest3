# IRIS-Viz Meta Quest 3

## Overview

This repository contains the Unity project for the IRIS-Viz Meta Quest 3 application. It is built using Unity's Universal Render Pipeline (URP) and is configured for XR development on the Android platform for Meta Quest devices.

This project integrates the IRIS SDK to connect with the IRIS visualization framework, allowing for immersive data interaction in virtual reality.

## Unity Project Setup

This project was set up following the general guidelines for creating an IRIS-Viz XR application. The key steps are:

1.  **Install Dependencies**: Requires a specific Unity LTS version with Android build support, the XR Interaction Toolkit, and the IRIS SDK `.unitypackage`.
2.  **Create the Project**: The project was initialized as a 3D (URP) project.
3.  **Configure Build Settings**: The build target is set to Android for Meta Quest 3. It includes the necessary XR plugins and API level configurations.
4.  **Import IRIS Assets**: The IRIS SDK and its sample scenes are included in this project's `Assets` directory. The `IrisAppConfig` is configured to connect to the appropriate environment.
5.  **Verify Setup**: The project is set up to be built and deployed to a target device.

## CI/CD

This repository includes a GitHub Actions workflow in `.github/workflows/build.yml` to automatically build the Unity application for the Android platform upon pushes to the `main` branch.

## Resources
*   [IRIS-Viz Meta Quest 3 Documentation](https://intuitive-robots.github.io/iris-project-page/xr_development/iris_viz.html)
*   [Main Python Client Repository](https://github.com/intuitive-robots/SimPublisher)
