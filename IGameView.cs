///* 
// * © 2024 Tobias Sachs
// * ViewBase
// * 11.07.2024 
//*/

//using System;
//using sachssoft.Sasogine.Core;
//using sachssoft.Sasogine.Providers;

//namespace sachssoft.Sasogine;

//public interface IGameView
//{
//    RuntimeBase Runtime { get; }

//    void BuildUI();

//    void Load();

//    void Unload();

//    void Update(GameContext context);

//    void Draw(GameContext context);
//}

//public interface IGameView<TRuntime> : IGameView where TRuntime : RuntimeBase
//{
//    new TRuntime Runtime { get; }

//}