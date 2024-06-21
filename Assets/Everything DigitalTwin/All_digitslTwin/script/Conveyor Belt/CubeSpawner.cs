using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab; // The cube prefab to spawn
    public float spawnRate = 1.0f; // Time interval between spawns
    public Vector3 cubeScale = new Vector3(1, 1, 1); // Scale of the spawned cubes
    public Color color1 = Color.red; // First color
    public Color color2 = Color.blue; // Second color

    private bool useColor1 = true; // Flag to alternate colors

    private void Start()
    {
        // Start the spawning coroutine
        StartCoroutine(SpawnCube());
    }

    private IEnumerator SpawnCube()
    {
        while (true)
        {
            // Spawn a new cube
            GameObject newCube = Instantiate(cubePrefab, transform.position, transform.rotation);

            // Set the scale of the new cube
            newCube.transform.localScale = cubeScale;

            // Alternate the color
            Renderer cubeRenderer = newCube.GetComponent<Renderer>();
            if (useColor1)
            {
                cubeRenderer.material.color = color1;
            }
            else
            {
                cubeRenderer.material.color = color2;
            }

            // Switch the color for the next spawn
            useColor1 = !useColor1;

            // Wait for the next spawn
            yield return new WaitForSeconds(spawnRate);
        }
    }
}