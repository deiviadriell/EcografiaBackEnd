using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPIEcoImagen;

namespace WebAPIEcoImagen.Controllers
{
    public class MedicosController : ApiController
    {
        private BDEcoimagenEntities db = new BDEcoimagenEntities();

        // GET: api/Medicos
        public IQueryable<Medico> GetMedicos()
        {
            return db.Medicos;
        }

        // GET: api/Medicos/5
        [ResponseType(typeof(Medico))]
        public IHttpActionResult GetMedico(int id)
        {
            Medico medico = db.Medicos.Find(id);
            if (medico == null)
            {
                return NotFound();
            }

            return Ok(medico);
        }

        // PUT: api/Medicos/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutMedico(int id, Medico medico)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != medico.idMedico)
            {
                return BadRequest();
            }

            db.Entry(medico).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MedicoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
        //agregar 
        // POST: api/Medicos
        [ResponseType(typeof(Medico))]
        public IHttpActionResult PostMedico(Medico medico)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Medicos.Add(medico);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = medico.idMedico }, medico);
        }

        // DELETE: api/Medicos/5
        [ResponseType(typeof(Medico))]
        public IHttpActionResult DeleteMedico(int id)
        {
            Medico medico = db.Medicos.Find(id);
            if (medico == null)
            {
                return NotFound();
            }

            db.Medicos.Remove(medico);
            db.SaveChanges();

            return Ok(medico);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MedicoExists(int id)
        {
            return db.Medicos.Count(e => e.idMedico == id) > 0;
        }
    }
}