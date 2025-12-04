namespace SetLight.AccesoADatos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncSchema : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Maintenance", "FinalizadoPor", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Maintenance", "FinalizadoPor");
        }
    }
}
