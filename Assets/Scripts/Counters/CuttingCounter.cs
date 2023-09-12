using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter , IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler OnCut;

    [SerializeField] private CuttingRecipesSO[] cutKitchenObjectSOArray;

    private int cuttingProgress;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            // There is no kitchenObject here
            if (player.HasKitchenObject())
            {
                // Player is carrying something
                if (HasRecipewWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    // Player carrying something that can be Cut
                    player.GetKitchenObject().SetkitchenObjectParent(this);
                    cuttingProgress = 0;

                    CuttingRecipesSO cuttingRecipeSO = GetCuttingRecpieSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
                    });
                }
                
            }
            else
            {
                // Player not carring anything

            }
        }
        else
        {
            // There is a KitchenObject here
            if (player.HasKitchenObject())
            {
                // Player is carrying something
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    // Player is holding a Plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                }
            }
            else
            {
                // Player is not carrying anything
                GetKitchenObject().SetkitchenObjectParent(player);
            }
        }
    }

    public override void InteractAlternate(Player player)
    {
        if(HasKitchenObject() && HasRecipewWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            // There is a Kitche Object here AND it can be cut
            cuttingProgress++;

            OnCut?.Invoke(this, EventArgs.Empty);

            CuttingRecipesSO cuttingRecipeSO = GetCuttingRecpieSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
            });

            if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
            {
                KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());
                GetKitchenObject().DestroySelf();

                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
            
        }
    }

    private bool HasRecipewWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipesSO cuttingRecipeSO = GetCuttingRecpieSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {   CuttingRecipesSO  cuttingRecipeSO = GetCuttingRecpieSOWithInput(inputKitchenObjectSO);
        if (cuttingRecipeSO != null)
        {
            return cuttingRecipeSO.output;
        }
        else
        {
            return null;
        }
      
    }


    private CuttingRecipesSO GetCuttingRecpieSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (CuttingRecipesSO cuttingRecipesSO in cutKitchenObjectSOArray)
        {
            if (cuttingRecipesSO.input == inputKitchenObjectSO)
            {
                return cuttingRecipesSO;
            }
        }
        return null;
    }
}
