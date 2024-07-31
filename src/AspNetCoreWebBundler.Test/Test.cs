using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCoreWebBundler.Test;

[TestClass]
public class Test
{
    private BundleProcessor _processor;

    [TestInitialize]
    public void Setup()
    {
        _processor = new BundleProcessor();

        if (Directory.Exists("artifacts/out/"))
        {
            Directory.Delete("artifacts/out/", true);
        }

        Directory.CreateDirectory("artifacts/out/");
    }

    [TestMethod]
    public void Test01()
    {
        var result = _processor.Process("artifacts/test01.json");
        Assert.IsTrue(result);

        var jsMinResult = File.ReadAllText("artifacts/out/test01.min.js");
        var cssMinResult = File.ReadAllText("artifacts/out/test01.min.css");
        var htmlMinResult = File.ReadAllText("artifacts/out/test01.min.html");
        
        Assert.IsTrue(jsMinResult.StartsWith("var file1=1,file2=2"));
        Assert.IsTrue(new FileInfo("artifacts/out/test01.min.js.map").Exists);

        Assert.AreEqual("body{background:url('/test.png')}body{display:block}body{background:url(../src/image.png?foo=hat)}", cssMinResult);
        Assert.AreEqual("<div>hatæ</div><span tabindex=2><i>hat</i></span>", htmlMinResult);
    }

    [TestMethod]
    public void Test02()
    {
        // Process with directory as source

        var result = _processor.Process("artifacts/test02.json");
        Assert.IsTrue(result);
        Assert.IsTrue(File.Exists("artifacts/out/test02.js"));

        var jsMinResult = File.ReadAllText("artifacts/out/test02.min.js");
        Assert.AreEqual("var file1=1,file2=2", jsMinResult);
    }

    [TestMethod]
    public void Test03()
    {
        // process gzip (minified and not)

        var result = _processor.Process("artifacts/test03.json");
        Assert.IsTrue(result);

        Assert.IsFalse(File.Exists("artifacts/out/test03.min.js"));
        Assert.IsTrue(File.Exists("artifacts/out/test03.js.gz"));

        Assert.IsFalse(File.Exists("artifacts/out/test03.2.js"));
        Assert.IsFalse(File.Exists("artifacts/out/test03.2.js.gz"));

        Assert.IsTrue(File.Exists("artifacts/out/test03.2.min.js"));
        Assert.IsTrue(File.Exists("artifacts/out/test03.2.min.js.gz"));
    }

    [TestMethod]
    public void Test04()
    {
        // html comments
        var result = _processor.Process("artifacts/test04.json");
        Assert.IsTrue(result);

        var html = File.ReadAllText("artifacts/out/test04.min.html");
        Assert.AreEqual("<div><!--ko if:observable--><p></p><!--/ko--></div>", html);

        var html2 = File.ReadAllText("artifacts/out/test04.2.min.html");
        Assert.AreEqual("<!-- This is a test HTML comment --><div><!--ko if:observable--><p></p><!--/ko--></div>", html2);
    }

    [TestMethod]
    public void Test12()
    {
        // globbing one folders

        var result = _processor.Process("artifacts/test12.json");
        Assert.IsTrue(result);

        Assert.AreEqual(File.ReadAllText("artifacts/out/test12.a.min.js"), "var a=1");
        Assert.AreEqual(File.ReadAllText("artifacts/out/test12.min.js"), "var a=1");
    }

    [TestMethod]
    public void Test13()
    {
        // globbing sub folders
        var result = _processor.Process("artifacts/test13.json");
        Assert.IsTrue(result);
        
        Assert.AreEqual(File.ReadAllText("artifacts/out/test13.min.js"), "var a=1,b=2");
        Assert.AreEqual(File.ReadAllText("artifacts/out/test13/a.min.js"), "var a=1");
        Assert.AreEqual(File.ReadAllText("artifacts/out/test13/sub/b.min.js"), "var b=2");
    }
}