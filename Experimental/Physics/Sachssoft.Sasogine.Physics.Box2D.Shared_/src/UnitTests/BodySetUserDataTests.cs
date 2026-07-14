using Box2D;

namespace UnitTests;

public class BodySetUserDataTests
{
    [Fact]
    public void ShouldBeAbleToSetValueTypeUserData()
    {
        string? error = null;
        Core.SetAssertFunction((condition, name, number) =>
        {
            error = condition;
            return 0;
        });

        WorldDef worldDef = WorldDef.Default;
        World world = World.CreateWorld(worldDef);
        
        BodyDef bodyDef = new BodyDef
        {
            Type = BodyType.Dynamic,
            Position = new System.Numerics.Vector2(0f, 0f)
        };
        
        Body body = world.CreateBody(bodyDef);
        
        // Set a value type as user data
        body.UserData = 42;

        // Retrieve and check the value
        Assert.Equal(42, body.UserData);

        if (error is not null) Assert.Fail(error);
    }
    
    [Fact]
    public void ShouldBeAbleToSetNintUserData()
    {
        string? error = null;
        Core.SetAssertFunction((condition, name, number) =>
        {
            error = condition;
            return 0;
        });

        WorldDef worldDef = WorldDef.Default;
        World world = World.CreateWorld(worldDef);
        
        BodyDef bodyDef = new BodyDef
            {
                Type = BodyType.Dynamic,
                Position = new System.Numerics.Vector2(0f, 0f)
            };
        
        Body body = world.CreateBody(bodyDef);
        
        nint data = 123456789;
        
        // Set a value type as user data
        body.UserData = data;

        // Retrieve and check the value
        Assert.Equal(data, body.UserData);

        if (error is not null) Assert.Fail(error);
    }
    
}