using System.Data.Entity.Migrations;

namespace SetLight.AccesoADatos.Migrations
{
    // Debe ser PUBLIC para que el proyecto UI pueda referenciarla.
    public sealed class Configuration : DbMigrationsConfiguration<SetLight.AccesoADatos.Contexto>
    {
        public Configuration()
        {
            // Usamos migraciones explícitas (recomendado para prod)
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;

            // El ContextKey no es necesario en EF6 a menos que tengas múltiples contextos;
            // si lo tenías por default, puedes omitirlo.
            // ContextKey = "SetLight.AccesoADatos.Contexto";
        }

        protected override void Seed(SetLight.AccesoADatos.Contexto context)
        {
            // Se ejecuta después de aplicar la última migración.
            // Ejemplos de seed (descomenta/ajusta si necesitas):
            //
            // context.Usuarios.AddOrUpdate(u => u.UserName,
            //     new Usuario { UserName = "admin@setlight.com", Email = "admin@setlight.com", ... }
            // );
        }
    }
}
