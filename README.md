ProjectX
=======

# Sciencenter Wall of Inspiration
An interactive, voice commands driven gallery based on Kinect 2.0 SDK

## Pre-requisites

Before running the code, ensure that the  following pre-requisites are installed on your machine:

 1. [Microsoft Visual Studio](https://www.visualstudio.com/downloads/ "Microsoft Visual Studio")
 2. [Kinect for Windows 2.0](https://www.microsoft.com/en-us/download/details.aspx?id=44561 "Kinect SDK 2.0")
 3. [Kinect Speech Recognition API](https://www.microsoft.com/en-us/download/details.aspx?id=27226 "Microsoft Speech Recognition SDK")
 4. [Kinect Speech Recognition Language Pack](https://www.microsoft.com/en-us/download/details.aspx?id=34809 "Kinect for Windows Language Pack")
 5. [Visual Studio Github plugin](https://visualstudio.github.com "Visual Studio Github Plugin")

## How to run?

Checkout the latest code from the master branch. Create a folder on Desktop named **sciencenterimages** that contains all the images required to run the slide show. The images are formatted to encode extra information. For example, if the image corresponds to Sir Isaac Newton and we want to keep it there for 5 seconds, the image will be named as *newton_5.jpg*. The application will pick up this information and break it down to adequate data structures. The ofﬁcial project website is maintained here.

## Coding

The project is written in C# and works on Windows platform. The code functionality is divided into three main parts:

 1. **GestureRecognition:** The main ﬁles that deal with gesture recognition are *GestureBase.cs*, *GestureEventArgs.cs*, *GestureHelper.cs*, *GestureRecognitionEngine.cs*, *GestureRecognitionResult.cs*, *GestureRecognitionResult.cs*, *GestureType.cs*, *SwipeToLeftGestureWithLeftHand.cs, SwipeToLeftGestureWithRightHand.cs, SwipeToRightGestureWithRightHand.cs*, and *SwipeToLeftGestureWithLeftHand.cs*
 2. **VoiceSearch:** The voice search code retrieves the training examples from the information contained in the image names. The relevant code for this is *MainWindow.xaml.cs*.
 3. **UserInterface:** The user interface consists of the entire slide show and the entire visual elements. The UI is deﬁned at MainWindow.xaml and the slide show code is deﬁned at *MainWindow.xaml.cs*.


**NOTE** - You need to compile the program for _x86_ or _x64_ architectures. This code does not work for _ARM_.
