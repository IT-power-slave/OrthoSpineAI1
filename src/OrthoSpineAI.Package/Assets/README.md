# Package Assets

Place the following PNG image files in this folder.
Required sizes (all must be present or the manifest validation will fail):

| File                   | Size (px)  |
|------------------------|------------|
| Square44x44Logo.png    | 44 × 44    |
| Square150x150Logo.png  | 150 × 150  |
| Wide310x150Logo.png    | 310 × 150  |
| StoreLogo.png          | 50 × 50    |
| SplashScreen.png       | 620 × 300  |

You can generate placeholder assets with Visual Studio:
  Right-click Package.appxmanifest → Visual Assets tab → Generate.

For CI/CD pipelines you can use the `MakeAppx` tool or the
`Microsoft.Windows.SDK.BuildTools` NuGet package.
