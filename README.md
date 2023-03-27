Stable Diffusion Plugin for Unity
=================================

Stable Diffusion plugin for Unity, based on [Apple's Core ML port]. You can run
the model on-editor and at-runtime without needing any extra components.

[Apple's Core ML port]: https://github.com/apple/ml-stable-diffusion

System Requirements
-------------------

- Unity 2023.1 or later
- Apple Silicon Mac (editor/runtime support)
- iPad Pro with Apple silicon (runtime support)

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
