using UnityEngine;
public class GamePresenter : MonoBehaviour
{
    private GameModel _model;
    private UIManager _view;
    
    public void Initialize(GameModel model, UIManager view)
    {
        _model = model;
        _view = view;
        
        // Suscribirse a eventos del modelo
        // Actualizar vista cuando cambie el modelo
    }
    
    public void AddScore(int points)
    {
        _model.AddScore(points);
        UpdateView();
    }
    
    private void UpdateView()
    {
        // Actualizar UI con datos del modelo
    }
}