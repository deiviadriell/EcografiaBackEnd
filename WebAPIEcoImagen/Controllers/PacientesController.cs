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
using LinqKit;
using WebAPIEcoImagen.Models;

namespace WebAPIEcoImagen.Controllers
{
    public class PacientesController : ApiController
    {
        private BDEcoimagenEntities db = new BDEcoimagenEntities();

        // GET: api/Pacientes
        public IQueryable<Paciente> GetPacientes()
        {
            return db.Pacientes;
        }

        // GET: api/Pacientes/5
        [ResponseType(typeof(Paciente))]
        public IHttpActionResult GetPaciente(int id)
        {
            Paciente paciente = db.Pacientes.Find(id);
            if (paciente == null)
            {
                return NotFound();
            }

            return Ok(paciente);
        }

        // PUT: api/Pacientes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutPaciente(int id, Paciente paciente)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != paciente.idPaciente)
            {
                return BadRequest();
            }

            db.Entry(paciente).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PacienteExists(id))
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

        // POST: api/Pacientes
        [ResponseType(typeof(Paciente))]
        public IHttpActionResult PostPaciente(Paciente paciente)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Pacientes.Add(paciente);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = paciente.idPaciente }, paciente);
        }
        [HttpGet]
        [Route("api/Pacientes/Parametro/{parametro}")]
        public IHttpActionResult GetSeguimientosFromPaciente(string parametro)
        {
            var paciente = db.Pacientes.Where(x => x.borrado == null && (x.nombres.Contains(parametro) || x.apellidos.Contains(parametro) || x.cedula.Contains(parametro) || x.correo.Contains(parametro) || x.direccion.Contains(parametro))).ToList();
            return Ok(paciente);
        }

        [HttpGet]
        [Route("api/Pacientes/Recientes/{totalPacientes}")]
        public IHttpActionResult GetPacientesRecientes(int totalPacientes)
        {
            var pacientes = db.Pacientes.OrderByDescending(x => x.idPaciente).Take(totalPacientes);
            return Ok(pacientes);
        }
        [HttpGet]
        [Route("api/Pacientes/Genero/{genero}")]
        public IHttpActionResult GetTotalPacientesFromGenero(string genero)
        {
            var paciente = db.Pacientes.Where(x => x.genero == genero).Count();
            return Ok(paciente);
        }
        [HttpGet]
        [Route("api/Pacientes/TotalPacientes/")]
        public IHttpActionResult GetTotalPacientes()
        {
            var pacientes = db.Pacientes.Select(x => x.idPaciente).Count();
            return Ok(pacientes);
        }

        [HttpGet]
        [Route("api/Pacientes/Seguimientos/{idPaciente}")]
        public IHttpActionResult GetSeguimientosFromPaciente(int idPaciente)
        {
            var seguimientos = db.Seguimientoes.Where(x => x.idPaciente == idPaciente).ToList();
            return Ok(seguimientos);
        }
        
        [Route("api/Pacientes/Graficos/Mes")]
        public IHttpActionResult GetGraficoPacientePorMes()
        {
            PacientesMesesNumero datos = new PacientesMesesNumero();
            var predicate = PredicateBuilder.New<Seguimiento>();

            List<string> meses = new List<string> { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            List<int> datosMeses = new List<int>();
            datos.meses = meses;
            int mesEntero = 1;
            int anoActual = DateTime.Now.Year;
            foreach (var dato in datos.meses)
            {
                int datoMes = db.Seguimientoes.Where(x => x.fecha.Value.Month == mesEntero && x.fecha.Value.Year == anoActual).Count();
                datosMeses.Add(datoMes);
                mesEntero++;                
            }
            datos.datos = datosMeses;
            return Ok(datos);            
            
        }
      
        // DELETE: api/Pacientes/5
        [ResponseType(typeof(Paciente))]
        public IHttpActionResult DeletePaciente(int id)
        {
            Paciente paciente = db.Pacientes.Find(id);
            if (paciente == null)
            {
                return NotFound();
            }

            db.Pacientes.Remove(paciente);
            db.SaveChanges();

            return Ok(paciente);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PacienteExists(int id)
        {
            return db.Pacientes.Count(e => e.idPaciente == id) > 0;
        }
    }
}