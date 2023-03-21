import CoreML
import StableDiffusion

final class Plugin {

    var pipeline: StableDiffusionPipeline

    var config: StableDiffusionPipeline.Configuration

    var generated: CGImage?

    // Create a pipeline and load resources into it
    init(resourcePath: String) throws {
        // Compute unit selection (all)
        let mlConfig = MLModelConfiguration()
        mlConfig.computeUnits = MLComputeUnits.all

        // Pipeline initialization
        let resourceURL = URL(filePath: resourcePath)
        pipeline = try StableDiffusionPipeline(resourcesAt: resourceURL,
                                             configuration: mlConfig,
                                             disableSafety: true,
                                              reduceMemory: false)
        try pipeline.loadResources()

        // Initial configuration
        config = StableDiffusionPipeline.Configuration(prompt: "")
        config.schedulerType = StableDiffusionScheduler.dpmSolverMultistepScheduler
    }

    // Run the pipeline and generate an image
    func runGenerator() throws {
        let images = try pipeline.generateImages(configuration: config)
        generated = images[0]!
    }
}

@_cdecl("SDCreate")
public func SDCreate(resourcePath: OpaquePointer) -> OpaquePointer! {
    let resourcePath = String(cString: UnsafePointer<CChar>(resourcePath))
    if let plugin = try? Plugin(resourcePath: resourcePath) {
        return OpaquePointer(Unmanaged.passRetained(plugin).toOpaque())
    }
    return nil;
}

@_cdecl("SDSetConfig")
public func SDSetConfig(_ plugin: OpaquePointer,
                          prompt: OpaquePointer,
                       stepCount: CInt,
                            seed: CInt,
                   guidanceScale: CFloat) {
    let plugin = Unmanaged<Plugin>.fromOpaque(UnsafeRawPointer(plugin)).takeUnretainedValue()
    plugin.config.prompt = String(cString: UnsafePointer<CChar>(prompt))
    plugin.config.stepCount = Int(stepCount)
    plugin.config.seed = UInt32(seed)
    plugin.config.guidanceScale = guidanceScale
}

@_cdecl("SDGenerate")
public func SDGenerate(_ plugin: OpaquePointer) {
    let plugin = Unmanaged<Plugin>.fromOpaque(UnsafeRawPointer(plugin)).takeUnretainedValue()
    try? plugin.runGenerator()
}

@_cdecl("SDGetImage")
public func SDGetImage(_ plugin: OpaquePointer) -> OpaquePointer! {
    let plugin = Unmanaged<Plugin>.fromOpaque(UnsafeRawPointer(plugin)).takeUnretainedValue()
    return OpaquePointer(CFDataGetBytePtr(plugin.generated!.dataProvider!.data))
}

@_cdecl("SDDestroy")
public func SDDestroy(_ plugin: OpaquePointer) {
    _ = Unmanaged<Plugin>.fromOpaque(UnsafeRawPointer(plugin)).takeRetainedValue()
}
