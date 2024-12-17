using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;

public class InstrGBSimManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> imagesPref = new List<GameObject>();
    [SerializeField]
    private List<GameObject> imageContainers=new List<GameObject>();
    [SerializeField]
    private bool continueGBSim = false;


    public static UnityAction<bool> setContinueGBSim;

    private void OnEnable()
    {
        setContinueGBSim += SetContinueGBSim;
    }
    private void OnDisable()
    {
        setContinueGBSim -= SetContinueGBSim;
        StopAllCoroutines();
    }

    private void SetContinueGBSim(bool value)
    {
        continueGBSim = value;
        StartCoroutine(SpawnImagesEvery5Sec());
    }
    private IEnumerator SpawnImagesEvery5Sec()
    {
        while(continueGBSim)
        {
            yield return new WaitForSeconds(5);

            int _rand_0 = Random.Range(0, 8);
            int _rand_1 = Random.Range(0, 8);
            int _rand_2 = Random.Range(0, 8);

            //Remove all old children for each container 
            foreach (GameObject image in imagesPref)
            {
                List<GameObject> gameObjects = new List<GameObject>();
                image.GetChildGameObjects(gameObjects);
                if(gameObjects.Count > 0)
                {
                    Destroy(image.GetComponentInChildren<RectTransform>().gameObject);    
                }
            }

            //Replace new children with the old removed children randomly
            GameObject _elem_0 = Instantiate(imagesPref[_rand_0], imageContainers[0].gameObject.GetComponent<RectTransform>());
            GameObject _elem_1 = Instantiate(imagesPref[_rand_1], imageContainers[1].gameObject.GetComponent<RectTransform>());
            GameObject _elem_2 = Instantiate(imagesPref[_rand_2], imageContainers[2].gameObject.GetComponent<RectTransform>());
        }
        
    }

}
