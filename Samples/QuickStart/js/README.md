Quick Start Using HTML5 and JScript
====================================

This quick start topic demonstrates how to create an app by using HTML5 and JScript.

## Create a new project

To create a new project, follow these steps:

1.  In Visual Studio, select **File** &gt; **New** &gt; **Project...**
2.  In the New Project window, select **Other Languages** &gt; **JavaScript** &gt; **Blank App (Windows Phone)**.
3.  Press **OK**.

A project has now been created for you.

## Include libraries to your project

Before starting to use the functionality provided by the SDK, the Lumia Imaging SDK libraries must be added to the project. For detailed instructions, see the chapter [Adding libraries to the project](../adding-libraries-to-the-project.html). Remember to remove the Any CPU configuration.

## Define your HTML5 UI

The UI we will build for this tutorial is very simple. There will be two HTML image controls and a button. One HTML image control will display the original image, and the other will display the filtered image.

Here are the steps to accomplish this:

1. Open the Package.appxmanifest, select the **Application** tab, and check the boxes for **Landscape** and **Landscape-flipped** under **Supported rotations**.
2. Open the **Capabilities** tab and check the box for **Pictures Library** capability.
3. In the Solution Explorer pane in Visual Studio, open the default.html.
4. Add all the controls that make our UI. In the HTML view, search for the **body** element.

HTML
```
<body class="phone">
  <p>Content goes here</p>
</body>
```

Replace the body and all its content with this code:

HTML
```
<body>
  <div id="contentGrid" >
    <div id="original">
      <img id="originalPhoto" src="" width="167" height="100" />
    </div>
    <div id="filtered">
      <img id="filteredPhoto" src="" width="100%" height="100%" />
    </div>
    <div id="buttons">
      <input id="loadButton" type="button" value="Pick an image" />
    </div>
  </div>
</body>
```

In the preceding code, we define the two preview images, filteredPhoto and originalPhoto, without specifying any source for the image to display. We will load the images later; code examples will also be given in the chapters below.

## Pick an image from the camera roll

Next, open the default.css in the css folder. Replace the content with the following:

CSS
```
body {
}

#contentGrid {
  display: -ms-grid;
  -ms-grid-rows: 85% 15%;
  height: 100%;
  width: 100%;
  background-color: #1e90ff;
}

#original {
  -ms-grid-row: 1;
  margin: 12px 0px 0px 12px;
  z-index: 99;
}

#filtered {
  -ms-grid-row: 1;
}

#buttons {
  -ms-grid-row: 2;
  -ms-grid-row-align: center;
  -ms-grid-column-align: center;
}

#loadButton {
  margin-left: 10px;
}
```

The code block above is for setting the layout of the screen.

![](../Images/samples/quickstart_wp81_choose_file.png)

To pick the image, we will use the **FileOpenPicker**, which is part of the Windows Phone 8.1 SDK. You should note that this code is different for Windows and Windows Phone. In Windows, you must use the **pickSingleFileAsync** method, and in Windows Phone, you would use the **pickSingleFileAndContinue** method. Open the default.js file inside the js folder and replace the **app.onactivated** with the following:


JScript
```
app.onactivated = function (args)
{
    if (args.detail.kind === activation.ActivationKind.launch)
    {
      if (args.detail.previousExecutionState !== activation.ApplicationExecutionState.terminated)
      {
          // TODO: This application has been newly launched. Initialize
          // your application here.
      }
      else
      {
          // TODO: This application has been reactivated from suspension.
          // Restore the application state here.
      }

      args.setPromise(WinJS.UI.processAll());

      var getPhotoButton = document.getElementById("loadButton");
      getPhotoButton.addEventListener("click", getPhotoButtonClickHandler, false);
    }
    else if (args.detail.kind == activation.ActivationKind.pickFileContinuation)
    {
        var file = args.detail.detail[0].files[0];
        var imageBlob = URL.createObjectURL(file);
        document.getElementById("originalPhoto").src = imageBlob;
        loadImage(file);
    }
};
```

**pickSingleFileAndContinue** will launch the file picker. When the user has chosen a photo, control is returned to the application through the **app.onactivated** method. We check if the activation was done through **FilePicker** by checking the parameter detail type against **ActivationKind.pickFileContinuation**. There we set the **originalPhoto** to show the image we just loaded, and call the **loadImage** method, which will have the Imaging SDK specific code.

To open the **FileOpenPicker**, we need to add the following method to the **default.js** file:

JScript
```
function getPhotoButtonClickHandler(args)
{
    var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
    openPicker.suggestedStartLocation =
      Windows.Storage.Pickers.PickerLocationId.picturesLibrary;
    openPicker.viewMode =
      Windows.Storage.Pickers.PickerViewMode.thumbnail;

    openPicker.fileTypeFilter.clear();
    openPicker.fileTypeFilter.append(".bmp");
    openPicker.fileTypeFilter.append(".png");
    openPicker.fileTypeFilter.append(".jpeg");
    openPicker.fileTypeFilter.append(".jpg");

    openPicker.pickSingleFileAndContinue();
}
```

## Use Effect to decode an image with an effect

Now, we have access to the file that the user selected, and we need to implement the filter and rendering to the image. In this tutorial, we apply the Cartoon filter and the Flip filter to the image, and then save it and display the resulting image in the HTML **img** control. The complete code of the **loadImage** becomes:

JScript
```
function loadImage(file)
{
    if (file)
    {
        var imageStream = new Lumia.Imaging.StorageFileImageSource(file);
        var cartoonEffect = new Lumia.Imaging.Artistic.CartoonEffect(imageStream);            
        var flipEffect = new Lumia.Imaging.Transforms.FlipEffect(cartoonEffect, Lumia.Imaging.Transforms.FlipMode.horizontal);            
        var renderer = new Lumia.Imaging.JpegRenderer(flipEffect);       

        // Create the filtered file and save it.
        renderer.renderAsync().then(function (buffer) {
            Windows.Storage.KnownFolders.picturesLibrary.createFileAsync("cartoon.jpg", Windows.Storage.CreationCollisionOption.replaceExisting).then(function (storagefile) {
                storagefile.openAsync(Windows.Storage.FileAccessMode.readWrite).then(function (storageStream) {
                    storageStream.writeAsync(buffer).then(function () {
                        storageStream.close();
                        var imageBlob = URL.createObjectURL(storagefile, { oneTimeOnly: true });
                        document.getElementById("filteredPhoto").src = imageBlob;
                    });
                });
            });
        });
    }
}
```

First, create the filters and set the image stream from the selected image. Then, create a list of filters which we pass to the **JpegRenderer**. Finally, save the filtered image and update the screen with the new image. The **JpegRenderer** takes the **FilterEffect** as a parameter to apply to the final image. After the rendering is done, we use the **createFileAsync** method to save the image as cartoon.jpg to the picture library and load it to the screen.

## Running the application

To run the application:

1. In the standard toolbar in Visual Studio, select **Device** or **Emulator**, and then select **Debug**.
2. Build the application.
3. Connect a device (if you are deploying it on device) and run.
4. Select an image using the **Pick an image** button.
5. You should see the main page that contains the decoded image with a Cartoon style filter effect added.


## Reference

[Lumia Imaging SDK](http://go.microsoft.com/fwlink/?LinkID=521939)