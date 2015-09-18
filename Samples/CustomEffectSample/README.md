CustomEffectSample
---------------------

This sample demonstrates  how to create **Custom Lumia Imaging Effects** to do image manipulation both on the CPU and the GPU.

Demonstrated in this sample is how to create:

	Manage effects
		1. GPU Pixel Shader Effect (CustomEffect.MagnifySmoothEffect)
		2. CPU Effect (CustomEffect.CustomGrayscaleEffect)
	
	Native C++ effects
		1. GPU Pixel Shader Effect (CustomNativeEffect.MagnifySmoothEffect)
		2. GPU Effect Wrapp a existing D2DEffect  (CustomNativeEffect.SplitToneEffect)
		3. GPU Pixel Shader Effect with TextureEffect (CustomNativeEffect.CustomGrayscaleEffect)
		4. CPU Effect (CustomNativeEffect.CustomGrayscaleEffect)
		
	 
The sample is split in to three parts **create a custom effect using manage code**, one part shows how to **create an effect using native C++ code.** The last part of the sample shows how to use the custom effects in an application. 

This sample will cover how to:

1.  **How to create an Manage effect in .NET**: use helper base classes (in Lumia.Imaging.Managed.dll).
	- Effects derive from **EffectBase**.
	- Effects are called upon to create **image workers**.
		- CPU-based image workers derive from **CpuImageWorkerBase**.
		- Direct2D-based image workers derive from **Direct2DPixelShaderImageWorkerBase**.

2.  **How to create an native effect in C++**: implement interfaces ( Lumia.Imaging.Managed.dll). 
	- Effects implement **IImageProvider2** and **IImageConsumer2**.
	- Effects are called upon to create **image workers**.
		- CPU-based image workers implement **ICpuImageWorker**.
		- Direct2D-based image workers implement either **IDirect2DImageWorker**, **IDirect2DPixelShaderImageWorker** *or* **IDirect2DWrappingImageWorker**.
		 

	

**Steps to create a manage effect in .net**

1. Create a Windows Runtime Component (Universal Windows)  project
2. Add **LumiaImagingSDK.uwp Nuget** to the project
3. If an GPU effect is created. Create a Visual C++ Windows Runtime Component (Universal Windows) to compile shaders (**CustomEffectShaderCompileProject** in the sample)
	1.  Delete the class1.h and class1.cpp 
	2.  Add a new Pixel Shader File(.hlsl)
	3.  Open properties on the file an select all properties change the **Object File name** to $(OutDir)%(Filename).pso
	4.  Add a custom build step to copy the file to the manage project. Open properties dialog for the project select *Build Events-> Post-Build Event-> Command Line and enter Copy $(OutDir)*.pso $(OutDir)..\..\CustomEffects\*.pso 
	4.  In *Additional Include Directories* add $(WindowsSDK_IncludePath)
	3.  Compile the the Pixel shader
	4.  Add the generated .pso file as link to the manage project and set it´s properties to embedded resource 
4. Add a new class **MyEffect** let the class inherit **EffectBase** (see MagnifySmoothEffect in sample)
	1. Override the functions  **SupportedRenderOptions**, CreateImageWorker and Clone.
5. Add a new class **MyImageWorker** let the class inherit **Direct2DPixelShaderImageWorkerBase** for GPU effect (see MagnifySmoothEffectDirect2DWorker in sample) or **CpuImageWorkerBase** for CPU effect(see CustomGrayscaleCpuWorker in sample)
	
See sample how to implement the GPU imageworker and how to **load the Pixel Shader** form the embedded resource file. Also make sure that the **Clone** of the effect is implemented correctly. 
**MyEffect** should be passed as a constructor parameter to the **MyDirect2DPixelShaderImageWorkerBase** set the **PixelShader** property on  **MyDirect2DPixelShaderImageWorkerBase**

CpuImageWorker should override the **OnProcess** method.


**Steps to create a native effect in C++**

1. Create a Visual C++ Windows Runtime Component (Universal Windows)  project
2. Add **LumiaImagingSDK.uwp Nuget** to the project
3. Open pch.h and add includude directive that are needed for Direct2D (see sample pch.h)
4. Open project properties go to *Linker->Input->Additional Dependencies* and add **dxguid.lib runtimeobject.lib**
5. If an GPU effect is created. Add a **Pixel Shader File(.hlsl)**
6. Open properties for that file and make sure that *HLSL->All Options->Header File Name* is set to %(FullPath).h
7. In *HLSL->All Options->Additional Include Directories* add $(WindowsSDK_IncludePath)
7. Add a new class MyEffect that implements **IImageProvider2** and **IImageConsumer2** (see MagnifySmoothEffect.h, MagnifySmoothEffect.cpp)
8. Add a new class MyImageWorker that implements **IDirect2DPixelShaderImageWorker** (see MagnifySmoothEffectDirect2DWorker.h, MagnifySmoothEffectDirect2DWorker.cpp)
9. Add the  #include "MyShader.hlsl.h" in MyImageWorker.cpp. "MyShader.hlsl.h" is generated when compiling the shader.



**Reference**

[Lumia Imaging SDK](https://dev.windows.com/en-us/featured/lumia)
[Direct2D Helpers](https://msdn.microsoft.com/en-us/library/windows/desktop/dn879811(v=vs.85).aspx)

## System requirements

**Client:** Windows 

**Server:** Windows 10 

**Phone:**  Windows 10 Technical Preview

## Build the sample

1.  Start Visual Studio 2015 and select **File** \> **Open** \> **Project/Solution**.
2.  Press Ctrl+Shift+B, or select **Build** \> **Build Solution**.

## Run the sample

The next steps depend on whether you just want to deploy the sample or you want to both deploy and run it.

**Deploying the sample:**

1.  Select **Build** \> **Deploy Solution**.

**Deploying and running the sample:**

1.  To debug the sample and then run it, press F5 or select **Debug** \> **Start Debugging**. To run the sample without debugging, press Ctrl+F5 or select **Debug** \> **Start Without Debugging**.


License
-------
See the **license.txt** file delivered with this project

