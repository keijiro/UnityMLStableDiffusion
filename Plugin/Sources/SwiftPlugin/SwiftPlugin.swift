import CoreGraphics
import CoreML
import Foundation
import StableDiffusion
import UniformTypeIdentifiers
import Cocoa

final class Plugin {
    init(resourcePath: String, prompt: String) throws {
        let config = MLModelConfiguration()
        config.computeUnits = MLComputeUnits.all
        let resourceURL = URL(filePath: resourcePath)

        let pipeline = try StableDiffusionPipeline(resourcesAt: resourceURL,
                                                   configuration: config,
                                                   disableSafety: true,
                                                   reduceMemory: false)
        try pipeline.loadResources()

        var pipelineConfig = StableDiffusionPipeline.Configuration(prompt: prompt)

        pipelineConfig.strength = 0.5
        pipelineConfig.imageCount = 1
        pipelineConfig.stepCount = 20
        pipelineConfig.seed = 100
        pipelineConfig.guidanceScale = 5
        pipelineConfig.schedulerType = StableDiffusionScheduler.dpmSolverMultistepScheduler
        pipelineConfig.rngType = StableDiffusionRNG.numpyRNG

        let images = try pipeline.generateImages(configuration: pipelineConfig)

        let outURL = URL(filePath: "Test.png")
        let image = images[0]
        let dest = CGImageDestinationCreateWithURL(outURL as CFURL, UTType.png.identifier as CFString, 1, nil)
        CGImageDestinationAddImage(dest!, image!, nil)
        CGImageDestinationFinalize(dest!)
    }
}

@_cdecl("plugin_create")
public func plugin_create(resourcePathPtr: OpaquePointer,
                          promptPtr: OpaquePointer) -> OpaquePointer! {
    do {
        let resourcePath = String(cString: UnsafePointer<CChar>(resourcePathPtr))
        let prompt = String(cString: UnsafePointer<CChar>(promptPtr))
        let type = try Plugin(resourcePath: resourcePath, prompt: prompt)
        let retained = Unmanaged.passRetained(type).toOpaque()
        return OpaquePointer(retained)
    } catch {
        return nil
    }
}

@_cdecl("plugin_destroy")
public func plugin_destroy(_ type: OpaquePointer) {
    _ = Unmanaged<Plugin>.fromOpaque(UnsafeRawPointer(type)).takeRetainedValue()
}
