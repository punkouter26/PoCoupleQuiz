using System;
using System.IO;
using Xunit;
using System.Text.RegularExpressions;

namespace PoCoupleQuiz.Tests
{
    public class ResponsiveDesignTests
    {
        private readonly string _cssContent; public ResponsiveDesignTests()
        {
            // Use an absolute path based on the solution directory
            string solutionDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
            string cssPath = Path.Combine(solutionDir, "PoCoupleQuiz.Client", "wwwroot", "wwwroot", "css", "app.css");
            _cssContent = File.ReadAllText(cssPath);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void CssFileExists()
        {
            Assert.NotNull(_cssContent);
            Assert.NotEmpty(_cssContent);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void HasMobileFirstMediaQueries()
        {
            // Check for mobile-first media queries
            Assert.Contains("@media (min-width:", _cssContent);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void HasResponsiveContainer()
        {
            // Check for responsive container class
            Assert.Contains(".container", _cssContent);
            Assert.Contains("width: 100%", _cssContent);
            Assert.Contains("max-width:", _cssContent);
        }
        [Trait("Category", "Unit")]
        [Fact]
        public void HasResponsiveGrid()
        {
            // Check for responsive layout elements
            Assert.Contains("@media", _cssContent);
            Assert.Contains("min-width:", _cssContent);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void HasTouchFriendlyElements()
        {
            // Check for touch-friendly button sizes
            Assert.Contains("padding: 0.75rem", _cssContent);
            Assert.Contains("font-size: 1rem", _cssContent);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void HasResponsiveBreakpoints()
        {
            // Check for standard breakpoints
            var breakpoints = new[] { "640px", "768px", "1024px" };
            foreach (var breakpoint in breakpoints)
            {
                Assert.Contains($"@media (min-width: {breakpoint})", _cssContent);
            }
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void HasFlexibleUnits()
        {
            // Check for use of relative units
            Assert.Contains("rem", _cssContent);
            Assert.Contains("em", _cssContent);
            Assert.Contains("%", _cssContent);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void HasResponsiveImages()
        {
            // Check for responsive image handling
            Assert.Contains("max-width: 100%", _cssContent);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void HasMobileNavigation()
        {
            // Check for mobile-friendly navigation
            Assert.Contains("display: flex", _cssContent);
            Assert.Contains("flex-direction:", _cssContent);
        }

        [Trait("Category", "Unit")]
        [Fact]
        public void HasResponsiveTypography()
        {
            // Check for responsive typography
            Assert.Contains("font-size:", _cssContent);
            Assert.Contains("line-height:", _cssContent);
        }
    }
}