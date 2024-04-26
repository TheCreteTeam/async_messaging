namespace WebApplication1.Data;

public class DbInitialiser 
{
    private readonly ApplicationDbContext _context;

    public DbInitialiser(ApplicationDbContext context) 
    {
        _context = context;
    }

    public void Run() 
    {
        // TODO: Add initialisation logic here.
        _context.Database.EnsureCreated();
    }
}
