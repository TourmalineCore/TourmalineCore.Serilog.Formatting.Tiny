module.exports = {
  branches: ['master', 'develop', {"name": "release/*", "prerelease": "alpha"}],
  plugins: [
    [
      '@semantic-release/commit-analyzer',
      {
        preset: 'conventionalcommits',
        parserOpts: {
          noteKeywords: ['BREAKING CHANGE', 'BREAKING CHANGES', 'BREAKING'],
        },
      },
    ],
    '@semantic-release/release-notes-generator',
    '@semantic-release/github',
    [
      '@semantic-release/changelog',
      {
        changelogFile: 'CHANGELOG.md',
        changelogTitle: '# Changelog',
      },
    ],
    [
      "@zedtk/semantic-release-nuget",
      {
        publish: true,
        projectRoot: './TourmalineCore.Serilog.Formatting.Tiny',
        includeSymbols: false,
      }
    ],
    [
      '@semantic-release/git',
      {
        assets: ['CHANGELOG.md', 'dist/*.nupkg', './TourmalineCore.Serilog.Formatting.Tiny/TourmalineCore.Serilog.Formatting.Tiny.csproj'],
        message: 'release: ${nextRelease.version}',
      },
    ],
  ],
};