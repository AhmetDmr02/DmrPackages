using System;
using System.Collections.Generic;
using System.Linq;

public class ObjectValueRegistry<TValue>
{
    // A dictionary to hold object references as keys and generic values as values
    private Dictionary<object, TValue> objectReferenceMap;

    public event Action<TValue> OnValueChanged; // An event to notify when a value is changed

    public ObjectValueRegistry()
    {
        objectReferenceMap = new Dictionary<object, TValue>(50);
    }
    public ObjectValueRegistry(int initialCapacity)
    {
        objectReferenceMap = new Dictionary<object, TValue>(initialCapacity);
    }
    public bool DoesHaveValue(TValue value)
    {
        Dictionary<object, TValue> copiedMap = new Dictionary<object, TValue>(objectReferenceMap).ToDictionary(x => x.Key, x => x.Value);

        foreach (var entry in copiedMap.Values)
        {
            if (entry.Equals(value))
            {
                return true;
            }
        }
        return false;
    }
    // Method to check if the object reference exists in the dictionary
    public bool Contains(object obj)
    {
        return objectReferenceMap.ContainsKey(obj);
    }

    // Method to add or update an object reference and its associated value
    public void Add(object obj, TValue value)
    {
        if (Contains(obj))
        {
            // If the object exists, update the value
            objectReferenceMap[obj] = value;

            OnValueChanged?.Invoke(value);
        }
        else
        {
            // If the object doesn't exist, add it to the dictionary
            objectReferenceMap.Add(obj, value);

            // Notify the event
            OnValueChanged?.Invoke(value);
        }
    }

    // Method to remove an object reference from the dictionary after checking if it exists
    public void Remove(object obj)
    {
        if (Contains(obj))
        {
            TValue val = objectReferenceMap[obj];

            objectReferenceMap.Remove(obj);

            OnValueChanged?.Invoke(val);
        }
    }

    // Display the contents of the dictionary
    public void DisplayContents()
    {
        Console.WriteLine("Object References and Values:");
        foreach (var entry in objectReferenceMap)
        {
            Console.WriteLine($"{entry.Key} -> {entry.Value}");
        }
    }
}
