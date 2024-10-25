using System.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;

class MyProgram
{
    public static async Task Run(CancellationToken token)
    {
        Scene myScene = new Scene();
        GameObject player = new GameObject("Player");
        GameObject enemy = new GameObject("Enemy");

        myScene.Add(player);
        myScene.Add(enemy);

        player.Add(new AIComponent());
        player.Add(new SoundComponent());
        player.Add(new PhysicsComponent());

        enemy.Add(new AIComponent());
        enemy.Add(new SoundComponent());
        enemy.Add(new PhysicsComponent());

        while (!token.IsCancellationRequested)
        {

            myScene.Update();
            Console.WriteLine("*********************************");
            // Wait for 2 seconds before the next update cycle.
            // You can set this to any time you want for instance
            // 33 ms to simulate 30 fps
            await Task.Delay(100);
        }
    }
}
//
abstract class Component
{
    protected GameObject owner;
    private bool isStarted = false;

    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
    }

    public virtual void Start() { }

    public virtual void Update()
    {
        if (!isStarted)
        {
            Start();
            isStarted = true;
        }
    }
}


class AIComponent : Component
{
    public override void Start()
    {
        Console.WriteLine(owner.Name + " AI Component Started!");
    }

    public override void Update()
    {
        base.Update();
        Console.WriteLine(owner.Name + " AI Component Updated!");
    }
}

class SoundComponent : Component
{
    
    public override void Start()
    {
        Console.WriteLine(owner.Name + " Sound Component Started!");
    }

    public override void Update()
    {
        base.Update();
        Console.WriteLine(owner.Name + " Sound Component Updated!");
    }
}

class PhysicsComponent : Component
{
    float acceleration = 0.0001f;
    public override void Start()
    {
        Console.WriteLine(owner.Name + " Physics Component Started!");
    }

    public override void Update()
    {
        base.Update();
        Console.WriteLine(owner.Name + " Physics Component Updated!");
        acceleration += acceleration;
        if (acceleration > 1000000)
        {
            acceleration = 0.0001f;
        }
        Console.WriteLine("The current acceleration is equal to " + acceleration);
    }
}

class GameObject
{
    public string Name { get; private set; }
    private List<Component> components = new List<Component>();

    public GameObject(string name)
    {
        this.Name = name;
    }

    public void Add(Component component)
    {
        component.SetOwner(this);
        components.Add(component);
    }

    public void Update()
    {
        Console.WriteLine($"{Name} is updating.");
        foreach (Component component in components)
        {
            component.Update();
        }
    }
}

class Scene
{
    private List<GameObject> gameObjects = new List<GameObject>();

    public void Add(GameObject gameObject)
    {
        gameObjects.Add(gameObject);
    }

    public void Update()
    {
        foreach (GameObject gameObject in gameObjects)
        {
            gameObject.Update();
        }
    }
}
