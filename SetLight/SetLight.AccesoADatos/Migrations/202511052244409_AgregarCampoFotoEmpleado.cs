namespace SetLight.AccesoADatos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AgregarCampoFotoEmpleado : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Empleado", "FotoPerfil", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Empleado", "FotoPerfil");
        }
    }
}
