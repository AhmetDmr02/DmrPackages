using DmrDependencyInjector;
using UnityEngine;

public class ReachObject : DIInjectOnlyMonoBehaviour
{
    [DmrInject][SerializeField] private TestRegisterable _testRegisterable;

    private new void Start()
    {
        base.Start();

        DontDestroyOnLoad(this);

        DIInjectorManager.InjectClassDependencies(this, out var injectResult);
    }

    private void Update()
    {
        if (!_injectResult.IsSuccess) return;
        _testRegisterable.Reach();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Always use DISceneManager to load scene if you are gonna use factory system
            DISceneManager.LoadScene(1);
        }
    }
}
