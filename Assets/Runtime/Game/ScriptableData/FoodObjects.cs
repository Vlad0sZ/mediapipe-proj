using System.Linq;
using Runtime.Infrastructure.Stacks;
using UnityEngine;

namespace Runtime.Game.ScriptableData
{
    [CreateAssetMenu(fileName = "Food Objects", menuName = "Game/Food Objects", order = 1)]
    public class FoodObjects : ScriptableObject
    {
        [System.Serializable]
        public sealed class FoodGroup
        {
            public string label;
            [SerializeField] private FoodWithIcon[] rightObjects;
            [SerializeField] private FoodWithIcon[] wrongObjects;

            public FoodWithIcon[] Rights => rightObjects;

            public FoodWithIcon[] Wrong => wrongObjects;
        }

        [SerializeField] private FoodGroup[] foodObjects;
        private IStack<FoodGroup> _objStack = null;

        public FoodGroup GetNextGroup()
        {
            _objStack ??= new ShuffledItemStack<FoodGroup>(foodObjects);
            return _objStack.GetNext();
        }

        public FoodWithIcon FindObject(string foodName)
        {
            var allObjects = foodObjects
                .SelectMany(x => x.Rights)
                .Union(foodObjects.SelectMany(x => x.Wrong))
                .Distinct();

            var firstFoundObject = allObjects
                .FirstOrDefault(x => x.IsName(foodName));

            return firstFoundObject;
        }
    }
}