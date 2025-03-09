using System;
using System.Collections.Generic;
using UnityEngine;

public class UIStack : MonoBehaviour
{
    public class WrongContextException : ApplicationException
    {
        private const string defaultMessage =
            "Calling or returning UIStack.Context instance is not the current context of its owner";
        public WrongContextException() : base(defaultMessage) {}
        public WrongContextException(Exception innerException) : base(defaultMessage, innerException) {}
        public WrongContextException(string message) : base(message) {}
        public WrongContextException(string message, Exception innerException) : base(message, innerException) {}
    }

    public class OwnerAlreadySetException: ApplicationException
    {
        private const string defaultMessage =
            "Owner already set (only the instantiating UIStack instance should set the owner of a UIStack.Context instance)";
        public OwnerAlreadySetException() : base(defaultMessage) {}
        public OwnerAlreadySetException(Exception innerException) : base(defaultMessage, innerException) {}
        public OwnerAlreadySetException(string message) : base(message) {}
        public OwnerAlreadySetException(string message, Exception innerException) : base(message, innerException) {}
    }

    public abstract class Context : MonoBehaviour
    {
        private UIStack owner = null;

        public void SetOwner(UIStack owner)
        {
            if (this.owner)
            {
                throw new OwnerAlreadySetException();
            }
            else
            {
                this.owner = owner;
            }
        }

        private void AssertIsContext()
        {
            if (owner.context != this)
            {
                throw new WrongContextException();
            }
        }

        public GameObject Call(GameObject uiPrefab)
        {
            AssertIsContext();
            return owner.Call(uiPrefab);
        }

        public T Call<T>(T uiPrefab) where T : Component
        {
            AssertIsContext();
            return owner.Call(uiPrefab);
        }

        public void Return(object returned = null)
        {
            AssertIsContext();
            owner.Return(returned);
        }

        public object returned
        {
            get
            {
                AssertIsContext();
                return owner.returned;
            }
        }
    }

    private InputModeManager inputMan;
    public InputModeManager.InputMode inputModeBeforeUI { get; private set; }
    private Stack<Context> stack;

    public Context context { get { return stack.Count <= 0 ? null : stack.Peek(); } }
    public object returned { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputMan = InputModeManager.Instance;
        inputModeBeforeUI = inputMan.inputMode;
        stack = new();
    }

    public GameObject Call(GameObject uiPrefab)
    {
        if (context)
        {
            context.gameObject.SetActive(false);
        }
        else
        {
            inputModeBeforeUI = inputMan.inputMode;
        }
        stack.Push(Instantiate(uiPrefab).GetComponent<Context>());
        context.SetOwner(this);
        context.transform.SetParent(this.transform, false);
        context.transform.SetLocalPositionAndRotation(3.0f*Vector3.back, Quaternion.identity);
        inputMan.SwitchControls(InputModeManager.InputMode.UI);
        return context.gameObject;
    }

    public T Call<T>(T uiPrefab) where T : Component
    {
        return Call(uiPrefab.gameObject).GetComponent<T>();
    }

    public void Return(object returned = null)
    {
        if (context)
        {
            Destroy(context.gameObject);
            stack.Pop();
            this.returned = returned;
            if (context)
            {
                context.gameObject.SetActive(true);
            }
            else
            {
                inputMan.SwitchControls(inputModeBeforeUI);
            }
        }
    }
}
