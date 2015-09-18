// For an introduction to the Blank template, see the following documentation:
// http://go.microsoft.com/fwlink/?LinkId=232509
(function () {
    "use strict";

    // Use this object to store info about the loaded image.
    var photoObject =
    {
        src: null,
        displayName: null,
        name: null,
        path: null,
        dateCreated: null
    };

    var app = WinJS.Application;
    var activation = Windows.ApplicationModel.Activation;

    app.onactivated = function (args) {
        if (args.detail.kind === activation.ActivationKind.launch) {
            if (args.detail.previousExecutionState !== activation.ApplicationExecutionState.terminated) {
                // TODO: This application has been newly launched. Initialize your application here.
            } else {
                // TODO: This application was suspended and then terminated.
                // To create a smooth user experience, restore application state here so that it looks like the app never stopped running.
            }
            args.setPromise(WinJS.UI.processAll());

            var getPhotoButton = document.getElementById("getPhotoButton");
            getPhotoButton.addEventListener("click", getPhotoButtonClickHandler, false);
        }
    };

    function getPhotoButtonClickHandler(eventInfo) {

        if (Windows.UI.ViewManagement.ApplicationView.value !=
            Windows.UI.ViewManagement.ApplicationViewState.snapped ||
            Windows.UI.ViewManagement.ApplicationView.tryUnsnap() === true) {

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

            openPicker.pickSingleFileAsync().done(
                loadImage,
                displayError);
        }
    }

    function loadImage(file) {
        if (file) {
            // Add picked file to MostRecentlyUsedList.
            WinJS.Application.sessionState.mruToken =
                Windows.Storage.AccessCache.StorageApplicationPermissions
                    .mostRecentlyUsedList.add(file);

            photoObject.displayName = file.displayName;
            photoObject.name = file.name;
            photoObject.path = file.path;
            photoObject.dateCreated = file.dateCreated;

          
            var imageStream = new Lumia.Imaging.StorageFileImageSource(file);
            var cartoonEffect = new Lumia.Imaging.Artistic.CartoonEffect(imageStream);            
            var flipEffect = new Lumia.Imaging.Transforms.FlipEffect(cartoonEffect, Lumia.Imaging.Transforms.FlipMode.horizontal);
            
            var renderer = new Lumia.Imaging.JpegRenderer(flipEffect);

            renderer.renderAsync().then(function (buffer) {
                Windows.Storage.KnownFolders.picturesLibrary.createFileAsync("cartoon.jpg", Windows.Storage.CreationCollisionOption.replaceExisting).then(function (storagefile) {
                    storagefile.openAsync(Windows.Storage.FileAccessMode.readWrite).then(function (storageStream) {
                        storageStream.writeAsync(buffer).then(function () {
                            storageStream.close();
                            var imageBlob = URL.createObjectURL(storagefile, { oneTimeOnly: true });
                            photoObject.src = imageBlob;

                            var contentGrid = document.getElementById("contentGrid");
                            WinJS.Binding.processAll(contentGrid, photoObject);
                        });
                    });
                });
            });
        }
    }

    function displayError(error) {
        document.getElementById("imageName").innerHTML = "Unable to load image.";
    }

    app.oncheckpoint = function (args) {
        // TODO: This application is about to be suspended. Save any state that needs to persist across suspensions here.
        // You might use the WinJS.Application.sessionState object, which is automatically saved and restored across suspension.
        // If you need to complete an asynchronous operation before your application is suspended, call args.setPromise().
    };

    app.start();
})();
