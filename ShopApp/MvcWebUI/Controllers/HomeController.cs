using DataAccess.Abstract;
using Microsoft.AspNetCore.Mvc;
using Entity;
using Business.Abstract;
using MvcWebUI.Models;

namespace MvcWebUI.Controllers
{
  public class HomeController : Controller
  {
    private IProductService _productService;

    public HomeController(IProductService productService)
    {
      this._productService = productService;
    }

    public IActionResult Index()
    {

      var productViewModel = new ProductListViewModel()
      {
        Products = _productService.GetHomePageProducts()
      };

      return View(productViewModel);
    }

    public IActionResult About()
    {

      return View();
    }

  }
}