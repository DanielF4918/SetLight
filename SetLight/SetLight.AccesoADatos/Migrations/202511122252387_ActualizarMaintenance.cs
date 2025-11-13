namespace SetLight.AccesoADatos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActualizarMaintenance : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Maintenance", "Comments", c => c.String(maxLength: 500));
            AddColumn("dbo.Maintenance", "Cost", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Maintenance", "EvidencePath", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Maintenance", "EvidencePath");
            DropColumn("dbo.Maintenance", "Cost");
            DropColumn("dbo.Maintenance", "Comments");
        }
    }
}
