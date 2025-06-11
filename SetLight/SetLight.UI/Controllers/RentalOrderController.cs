using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SetLight.UI.Controllers
{
    public class RentalOrderController : Controller
    {
        // GET: RentalOrder
        public ActionResult Index()
        {
            return View();
        }

        // GET: RentalOrder/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: RentalOrder/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RentalOrder/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: RentalOrder/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: RentalOrder/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: RentalOrder/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: RentalOrder/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
