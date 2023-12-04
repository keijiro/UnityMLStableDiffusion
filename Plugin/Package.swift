// swift-tools-version: 5.7

import PackageDescription

let package = Package(
    name: "StableDiffusionPlugin",
    platforms: [
        .macOS("13.1"),
        .iOS("16.2")
    ],
    products: [
        .library(
            name: "StableDiffusionPlugin",
            type: .dynamic,
            targets: ["StableDiffusionPlugin"]),
    ],
    dependencies: [
        .package(url: "https://github.com/apple/ml-stable-diffusion.git", from: "1.1.0")
    ],
    targets: [
        .target(
            name: "StableDiffusionPlugin",
            dependencies: [
                .product(name: "StableDiffusion", package: "ml-stable-diffusion")
            ])
    ]
)
