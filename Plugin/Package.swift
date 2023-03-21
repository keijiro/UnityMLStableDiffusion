// swift-tools-version: 5.7

import PackageDescription

let package = Package(
    name: "SwiftPlugin",
    platforms: [
        .macOS("13.1")
    ],
    products: [
        .library(
            name: "SwiftPlugin",
            type: .dynamic,
            targets: ["SwiftPlugin"]),
    ],
    dependencies: [
        .package(url: "https://github.com/apple/ml-stable-diffusion.git", branch: "main")
    ],
    targets: [
        .target(
            name: "SwiftPlugin",
            dependencies: [
                .product(name: "StableDiffusion", package: "ml-stable-diffusion")
            ])
    ]
)
