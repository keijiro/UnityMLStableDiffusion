import CoreGraphics
import CoreML
import Foundation
import StableDiffusion
import UniformTypeIdentifiers
import Cocoa

final class Plugin {
    var pipeline: StableDiffusionPipeline
    var pipelineConfig: StableDiffusionPipeline.Configuration
    var generatedImage: CGImage?

    init(resourcePath: String) throws {
        let config = MLModelConfiguration()
        config.computeUnits = MLComputeUnits.all
        let resourceURL = URL(filePath: resourcePath)

        pipeline = try StableDiffusionPipeline(resourcesAt: resourceURL,
                                                   configuration: config,
                                                   disableSafety: true,
                                                   reduceMemory: false)
        try pipeline.loadResources()

        pipelineConfig = StableDiffusionPipeline.Configuration(prompt: "test")
        pipelineConfig.strength = 0.5
        pipelineConfig.imageCount = 1
        pipelineConfig.schedulerType = StableDiffusionScheduler.dpmSolverMultistepScheduler
        pipelineConfig.rngType = StableDiffusionRNG.numpyRNG
    }

    func generate() throws {
        let images = try pipeline.generateImages(configuration: pipelineConfig)
        generatedImage = images[0]!
    }
}

@_cdecl("plugin_create")
public func plugin_create(resourcePathPtr: OpaquePointer) -> OpaquePointer! {
    do {
        let resourcePath = String(cString: UnsafePointer<CChar>(resourcePathPtr))
        let type = try Plugin(resourcePath: resourcePath)
        let retained = Unmanaged.passRetained(type).toOpaque()
        return OpaquePointer(retained)
    } catch {
        return nil
    }
}

@_cdecl("plugin_set_config")
public func plugin_set_config(_ type: OpaquePointer,
                           promptPtr: OpaquePointer,
                           stepCount: CInt,
                                seed: CInt,
                       guidanceScale: CFloat) {
    let type = Unmanaged<Plugin>.fromOpaque(UnsafeRawPointer(type)).takeUnretainedValue()
    let prompt = String(cString: UnsafePointer<CChar>(promptPtr))
    type.pipelineConfig.prompt = prompt
    type.pipelineConfig.stepCount = Int(stepCount)
    type.pipelineConfig.seed = UInt32(seed)
    type.pipelineConfig.guidanceScale = guidanceScale
}

@_cdecl("plugin_generate")
public func plugin_generate(_ type: OpaquePointer) {
    do {
        let type = Unmanaged<Plugin>.fromOpaque(UnsafeRawPointer(type)).takeUnretainedValue()
        try type.generate()
    } catch {
    }
}

@_cdecl("plugin_get_image")
public func plugin_get_image(_ type: OpaquePointer) -> OpaquePointer! {
    let type = Unmanaged<Plugin>.fromOpaque(UnsafeRawPointer(type)).takeUnretainedValue()
    let raw = CFDataGetBytePtr(type.generatedImage!.dataProvider!.data)
    return OpaquePointer(raw)
}

@_cdecl("plugin_destroy")
public func plugin_destroy(_ type: OpaquePointer) {
    _ = Unmanaged<Plugin>.fromOpaque(UnsafeRawPointer(type)).takeRetainedValue()
}
