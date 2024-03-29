Unity Core ML Stable Diffusion Plugin
=====================================

![gif](https://user-images.githubusercontent.com/343936/228759539-a35a37f2-77d6-4a10-8392-d875b968fea6.gif)

Stable Diffusion plugin for Unity, based on [Apple's Core ML port]. You can run
the model on-editor and at-runtime without needing any extra components.

[Apple's Core ML port]: https://github.com/apple/ml-stable-diffusion

System Requirements
-------------------

- Unity 2023.1 or later
- Apple Silicon Mac (editor/runtime support) with macOS 13.1 or later
- iPad Pro with Apple silicon (runtime support) with iOS 16.2 or later

Although the plugin supports iOS, it requires huge amount of memory to run the
model, so it only supports memory-rich iPad models.

How To Try
----------

Before running the sample project, you must put the model files in the
`Assets/StreamingAssets` directory.

- Clone or download the [pre-converted Stable Diffusion 2 model repository].
- Copy the `split_einsum/compiled` directory into `Assets/StreamingAssets`.
- Rename the directory to `StableDiffusion`.

[pre-converted Stable Diffusion 2 model repository]:
  https://huggingface.co/apple/coreml-stable-diffusion-2-base

It takes a long time (a few minutes) for the first run. After this
initialization step, it only takes a few tens of seconds to generate an image.

Performance Considerations
--------------------------

You can change which processing unit it uses by switching the "Compute Units"
property in the Tester component.

It depends on the device model to choose the best option:

- M1/M2 Mac and iOS: GPUs in those devices are not powerful enough, so you
  should select "CPU and NE" (Neural Engine) or "All".
- M1/M2 Pro/Max Mac: GPUs in those devices have enough processing power
  compared to NE, so "CPU and GPU" can be a better option.

When using "CPU and GPU" mode, you must use the "original" model instead of
the "split_einsum" model. Please overwrite the `StreamingAssets/StableDiffusion`
directory with the `original/compiled` directory.

LCM (SD-Turbo) Support
----------------------

You can use [SD-Turbo] or other LCMs (latent consistency models) with setting
`Pipeline.Scheduler` to `Lcm`. You might also have to change `StepCount` to 1\~4
and `GuidanceScale` to 1\~2. Please refer to the model description to know the
correct settings.

You can download the [pre-converted SD-Turbo model] from my Hugging Face
repository.

[SD-Turbo]: https://huggingface.co/stabilityai/sd-turbo
[pre-converted SD-Turbo model]:
  https://huggingface.co/keijiro-tk/coreml-sd-turbo

Sample Projects
---------------

![gif](https://user-images.githubusercontent.com/343936/228760795-9e712684-2ee6-4e63-9241-06d8aa125a17.gif)

- [Flipbook3](https://github.com/keijiro/Flipbook3): Running the image-to-image
  pipeline with a real-time 3D scene.
