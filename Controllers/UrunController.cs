using dotnet_store.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_store.Controllers;

public class UrunController : Controller
{
    // Dependecy Injection => DI
    private readonly DataContext _context;
    public UrunController(DataContext context)
    {
        _context = context;
    }

    public ActionResult Index ()
    {
        var urunler = _context.Urunler.ToList();
        return View(urunler);
    }

    public ActionResult List(string url)
    {
        var urunler = _context.Urunler.Where(i => i.Aktif && i.Kategori.Url == url).ToList();
        return View(urunler);
    }

    public ActionResult Details(int id)
    {
        var urun = _context.Urunler.FirstOrDefault(i => i.Id == id);
        // var urun = _context.Urunler.Find(id); 
        if (urun == null)
        {
            return RedirectToAction("Index","Home");
        }
        ViewData["BenzerUrunler"] = _context.Urunler
                                            .Where(i => i.Aktif && i.KategoriId == urun.KategoriId && i.Id != urun.Id)
                                            .Take(4)
                                            .ToList();
        return View(urun);
    }
}