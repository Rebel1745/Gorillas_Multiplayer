using Unity.Netcode;
using UnityEngine;

public class Building : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer _buildingSpriteRenderer;
    [SerializeField] private Color _buildingSpriteColour;

    [Rpc(SendTo.NotServer)]
    public void UpdateBuildingSpriteColourRpc()
    {
        _buildingSpriteRenderer.color = _buildingSpriteColour;
    }

    [Rpc(SendTo.NotServer)]
    public void SetBuildingSpriteColourRpc(Color color)
    {
        _buildingSpriteColour = color;
    }
}
