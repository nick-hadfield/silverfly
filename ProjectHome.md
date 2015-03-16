A polymorphic publish/subscribe event framework for .Net that makes use of weak references to simplify event management.

Instead of raising events you can instead publish objects.

```
bus.Publish(this, new Dog());
bus.Publish(this, new Cat());
```

Other classes might then subscribe to specific types of notifications. eg...

```
bus.Subscribe<Mammal>(
   (sender, notification) =>
   {
      Console.WriteLine(notification.Description);
   }
);
```