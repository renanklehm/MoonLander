

internal class Game
{
    public Lander lander;
    public float deltaTime = 1f;
    public float gravityAcceleration = 1.62f;
    public float maxTouchDownSpeed = 2f;

    private static void Main(string[] args)
    {
        Game game = new Game();
        game.Start();
        game.Update();
    }

    public void Start()
    {
        FuelTank[] tanks = new FuelTank[2]
        {
            new FuelTank(maxFuel: 10000, dryMass: 3000),
            new FuelTank(maxFuel: 5000, dryMass: 2000)
        };
        Engine[] engines = new Engine[2]
        {
            new Engine(
                maxMassFlow: 30,
                minMassFlow: 0,
                exhaustVelocity: 8000,
                dryMass: 8000,
                fuelTanks: tanks
            ),
            new Engine(
                maxMassFlow: 30,
                minMassFlow: 0,
                exhaustVelocity: 8000,
                dryMass: 8000,
                fuelTanks: tanks
            ),

        };
        lander = new Lander(
            dryMass: 30000,
            engineCluster: engines,
            fuelTanks: tanks,
            initialState: new StateVector(
                position: 1000,
                velocity: 0,
                acceleration: 0
            )
        );
    }

    public void Update()
    {
        while (lander.state.position > 0)
        {
            Console.WriteLine("Every time you press enter, the game advances " + deltaTime.ToString("0.00") + "s");
            PrintStatus();
            Console.WriteLine("How much throttle? (Percentage between 0 and 1)");

            string throttleInput = Console.ReadLine();
            float throttle;
            try
            {
                throttle = Math.Min(float.Parse(throttleInput), 1f);
            }
            catch (FormatException)
            {
                throttle = 0f;
            }

            lander.state.acceleration = -gravityAcceleration;
            lander.Burn(throttle, deltaTime);
            Console.Clear();
        }

        PrintStatus();
        Console.WriteLine("---------------------------------------");

        if (Math.Abs(lander.state.velocity) > maxTouchDownSpeed)
        {
            Console.WriteLine("Sorry... You crashed!");
        }
        else
        {
            Console.WriteLine("You landed safely!");
        }
        Console.WriteLine("Do you want to play again? (Y/N)");
        string playAgainInput = Console.ReadLine();
        if (playAgainInput == "Y" || playAgainInput == "y")
        {
            Start();
            Update();
            Console.Clear();
        }
    }




    private void PrintStatus()
    {
        Console.WriteLine("Height: " + lander.state.position);
        Console.WriteLine("Velocity: " + lander.state.velocity);
        Console.WriteLine("Acceleration: " + lander.state.acceleration);
        Console.WriteLine("Total mass: " + lander.totalMass);
        Console.WriteLine("Fuel mass: " + lander.fuelMass);
    }
}

public struct StateVector
{
    public float position;
    public float velocity;
    public float acceleration;

    public StateVector(float position, float velocity, float acceleration)
    {
        this.position = position;
        this.velocity = velocity;
        this.acceleration = acceleration;
    }
}

public class Lander
{
    public float dryMass;
    public StateVector state;
    public Engine[] engineCluster;
    public FuelTank[] fuelTanks;

    public float totalMass
    {
        get
        {
            float totalMass = dryMass;
            foreach (Engine engine in engineCluster)
            {
                totalMass += engine.dryMass;
            }
            totalMass += fuelMass;
            return totalMass;
        }
    }

    public float fuelMass
    {
        get
        {
            float fuelMass = 0;
            foreach (FuelTank tank in fuelTanks)
            {
                fuelMass += tank.mass;
            }
            return fuelMass;
        }
    }

    public Lander(float dryMass, Engine[] engineCluster, FuelTank[] fuelTanks, StateVector initialState)
    {
        this.dryMass = dryMass;
        this.engineCluster = engineCluster;
        this.fuelTanks = fuelTanks;
        this.state = initialState;
    }

    public void Burn(float throttle, float deltaTime)
    {
        float totalThrust = 0;
        foreach (Engine engine in engineCluster)
        {
            totalThrust += engine.Fire(throttle, deltaTime);
        }
        state.acceleration += totalThrust / totalMass;
        state.velocity = state.velocity + state.acceleration * deltaTime;
        state.position = state.position + state.velocity;
    }
}

public class Engine
{
    public float maxMassFlow;
    public float minMassFlow;
    public float exhaustVelocity;
    public float exhaustPressure;
    public float exhaustCrossSectionalArea;
    public float dryMass;
    private FuelTank[] fuelTanks;

    public Engine(
        float maxMassFlow, 
        float minMassFlow, 
        float exhaustVelocity,
        float dryMass,
        FuelTank[] fuelTanks)
    {
        this.maxMassFlow = maxMassFlow;
        this.minMassFlow = minMassFlow;
        this.exhaustVelocity = exhaustVelocity;
        this.dryMass = dryMass;
        this.fuelTanks = fuelTanks;
    }

    public float Fire(float throttle, float deltaTime)
    {
        float desiredMassFlow = (minMassFlow + (maxMassFlow - minMassFlow) * throttle) * deltaTime;
        Console.WriteLine("Desired mass flow: " + desiredMassFlow);
        float actualMassFlow = 0;
        foreach (FuelTank tank in fuelTanks)
        {
            actualMassFlow += tank.Drain(desiredMassFlow);
            desiredMassFlow -= actualMassFlow;
            if (desiredMassFlow <= 0)
            {
                break;
            }
        }
        return actualMassFlow * exhaustVelocity;
    }
}

public class FuelTank
{
    public float maxFuel;
    public float dryMass;
    public float currentFuel;

    public float mass
    {
        get
        {
            return dryMass + currentFuel;
        }
    }

    public FuelTank(float maxFuel, float dryMass)
    {
        this.maxFuel = maxFuel;
        this.dryMass = dryMass;
        currentFuel = maxFuel;
    }

    public float Drain(float amount)
    {
        float drainedAmount = Math.Min(amount, currentFuel);
        currentFuel -= drainedAmount;
        return drainedAmount;
    }

    public bool IsEmpty()
    {
        return currentFuel <= 0;
    }
}