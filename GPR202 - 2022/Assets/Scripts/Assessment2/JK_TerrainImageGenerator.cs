using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JK_TerrainImageGenerator : MonoBehaviour
{
    public Color grasslandsZone; // 0.0 -> 0.2
    public Color swampZone;      // 0.2 -> 0.4
    public Color forrestZone;    // 0.4 -> 0.6
    public Color mountainZone;   // 0.6 -> 0.8
    public Color lavaZone;       // 0.8 -> 1.0
    public Color waterCell;

    public GameObject voxelPrefab;

    public float heightScalar = 1.0f;

    public int seed = 42069;

    public int ZONE_LAYER = 0;
    public int RANDOM_ZONE_LAYER = 1;
    public int RIVER_LAYER = 2;

    SpriteRenderer renderer;
    Sprite sourceSprite;
    Texture2D sourceTexture;

    float timeUntilNextGen = 0.0f;
    float timeBetweenGens = 10.0f;

    List<GameObject> voxels = new List<GameObject>();

    // The number of cycles of the basic noise pattern that are repeated
    // over the width and height of the texture.
    public float scale = 1.0F;

    private float Noise(int x, int y, int layer, int seed)
    {
        Random.seed = 1000000;
        int randomValue1 = (int)Random.Range(1.0f, 10000.0f);
        Random.seed = 232323;
        int randomValue2 = (int)Random.Range(1.0f, 10000.0f);
        Random.seed = 98989989;
        int randomValue3 = (int)Random.Range(1.0f, 10000.0f);
        Random.seed = 123123123;
        int randomValue4 = (int)Random.Range(1.0f, 10000.0f);

        Random.seed = (x + randomValue1) * (y + randomValue2) * (layer + randomValue3) * (seed + randomValue4);
        float noise = Random.Range(0.0f, 0.999999999999999f);
        return noise;
    }

    Color NoiseToZone(float noiseValue)
    {
        if (noiseValue < 0.25f) // 25%
        {
            // grass
            return grasslandsZone;
        }
        else if (noiseValue < 0.45f) // 20%
        {
            // swamp
            return swampZone;
        }
        else if (noiseValue < 0.65f) // 20%
        {
            // forest
            return forrestZone;
        }
        else if (noiseValue < 0.90f) // 20%
        {
            // mountain
            return mountainZone;
        }
        else // 15%
        {
            // lava
            return lavaZone;
        }
    }

    void BlackOut()
    {
        for (int y = 0; y < sourceTexture.height; ++y)
        {
            for (int x = 0; x < sourceTexture.width; ++x)
            {
                sourceTexture.SetPixel(x, y, new Color(0.0f, 0.0f, 0.0f, 0.0f));
            }
        }
        sourceTexture.Apply();
    }

    private void Update()
    {
        timeUntilNextGen -= Time.deltaTime;
        if (timeUntilNextGen > 0.0f) return;

        seed = (int)Random.Range(0, 1000000.0f);

        timeUntilNextGen = timeBetweenGens;

        renderer = GetComponent<SpriteRenderer>();
        sourceSprite = renderer.sprite;
        sourceTexture = sourceSprite.texture;

        //BlackOut();
        //GenerateVoxelHypemap();
        //GenerateVoxelTerrain();

        GenerateBasicRegions();
        RemoveRegionsOfOneCell();

        /*
        
        BlurTerrain(3);
        GenerateRiver(101 + seed);
        GenerateRiver(30203 + seed);
        GenerateRiver(123123 + seed);
        GenerateRiver(444224 + seed);
        GenerateRiver(4423 + seed);
        GenerateRiver(4156661 + seed);
        GenerateRiver(6666446 + seed);
        */
        print("DONE");
        // remove areas that dont have 10+ tiles (small zones).
        // change zones that shouldnt touch.

        Sprite newSprite = Sprite.Create(sourceTexture, sourceSprite.rect, new Vector2(0.5f, 0.5f));
        renderer.sprite = newSprite;
    }

    void GenerateBasicRegions()
    {
        // Generate original zone type
        float originalZoneType = Noise(0, 0, ZONE_LAYER, seed);
        sourceTexture.SetPixel(0, 0, NoiseToZone(originalZoneType));

        for (int y = 0; y < sourceTexture.height; ++y)
        {
            for (int x = 0; x < sourceTexture.width; ++x)
            {
                if (x == 0 && y == 0) continue; // skip the original zone.

                Color l = sourceTexture.GetPixel(x - 1, y);
                Color bl = sourceTexture.GetPixel(x - 1, y - 1);
                Color b = sourceTexture.GetPixel(x, y - 1);
                Color br = sourceTexture.GetPixel(x + 1, y - 1);

                float randomZoneType = Noise(x, y, ZONE_LAYER, seed);
                Color randomZoneColor = NoiseToZone(randomZoneType);

                Color randomlyChosen = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                while (randomlyChosen == new Color(0.0f, 0.0f, 0.0f, 0.0f))
                {
                    Color[] bagOfZones = { l, bl, b, br, l, bl, b, br, l, bl, b, br, l, bl, b, br, randomZoneColor };
                    float randomNumber = Noise(x, y, RANDOM_ZONE_LAYER, seed);
                    int randomIndex = (int)Mathf.Floor(randomNumber * bagOfZones.Length);
                    randomlyChosen = bagOfZones[randomIndex];
                }


                sourceTexture.SetPixel(x, y, randomlyChosen);
            }
        }
        sourceTexture.Apply();
    }

    void RemoveRegionsOfOneCell()
    {
        print(sourceTexture.GetPixel(0, 0));

        // remove all tiles that do not have neighbouring tiles of the same type.
        for (int y = 0; y < sourceTexture.height; ++y)
        {
            for (int x = 0; x < sourceTexture.width; ++x)
            {
                Color c = sourceTexture.GetPixel(x, y);
                Color tl = sourceTexture.GetPixel(x - 1, y + 1);
                Color t = sourceTexture.GetPixel(x, y + 1);
                Color tr = sourceTexture.GetPixel(x + 1, y + 1);
                Color l = sourceTexture.GetPixel(x - 1, y);
                Color r = sourceTexture.GetPixel(x + 1, y);
                Color bl = sourceTexture.GetPixel(x - 1, y - 1);
                Color b = sourceTexture.GetPixel(x, y - 1);
                Color br = sourceTexture.GetPixel(x + 1, y - 1);

                if (c == tl || c == t || c == tr || c == l || c == r || c == bl || c == b || c == br)
                {
                    continue;
                }

                sourceTexture.SetPixel(x, y, b);
            }
        }
        sourceTexture.Apply();
    }


    void BlurTerrain(int kernelWidth)
    {
        if (kernelWidth % 2 == 0) return; // this means the number is event, ignore.

        Texture2D blurTexture = new Texture2D(sourceTexture.width, sourceTexture.height);

        // Re-generate
        for (int y = 0; y < sourceTexture.height; ++y)
        {
            for (int x = 0; x < sourceTexture.width; ++x)
            {
                int yBottomRow = y - ((kernelWidth - 1) / 2);
                int yTopRow = y + ((kernelWidth - 1) / 2);

                int xLeftColumn = x - ((kernelWidth - 1) / 2);
                int xRightColumn = x + ((kernelWidth - 1) / 2);

                int samples = 0;           // number of pixels sampled in the kernel
                Color sampleAdded = new Color(); // add all pixels in the kernel here
                for (int yOffset = yBottomRow; yOffset <= yTopRow; ++yOffset)
                {
                    for (int xOffset = xLeftColumn; xOffset <= xRightColumn; ++xOffset)
                    {
                        Color sample = sourceTexture.GetPixel(xOffset, yOffset);
                        sampleAdded += sample;
                        samples += 1;
                    }
                }

                Color averaged = sampleAdded / samples;
                blurTexture.SetPixel(x, y, averaged);
            }
        }
        blurTexture.Apply();

        for (int y = 0; y < sourceTexture.height; ++y)
        {
            for (int x = 0; x < sourceTexture.width; ++x)
            {
                sourceTexture.SetPixel(x, y, blurTexture.GetPixel(x, y));
            }
        }
        sourceTexture.Apply();
    }

    struct GeneratedRiverCell
    {
        public int x;
        public int y;
        public int direction;
        // 0 is up
        // 1 is up diag
        // 2 is fwd
        // 3 is down diag
        // 4 is down
    };

    void GenerateRiver(int offset)
    {
        LinkedList<GeneratedRiverCell> generatedRiverCells = new LinkedList<GeneratedRiverCell>();

        int seed = 362122325 + offset;

        // Generate a random start pixel on a random edge.

        // 0 is up
        int upOffsetX = 0;
        int upOffsetY = 0;
        // 1 is up diag
        int upDiagOffsetX = 0;
        int upDiagOffsetY = 0;
        // 2 is fwd
        int fwdOffsetX = 0;
        int fwdOffsetY = 0;
        // 3 is down diag
        int downDiagOffsetX = 0;
        int downDiagOffsetY = 0;
        // 4 is down
        int downOffsetX = 0;
        int downOffsetY = 0;

        int randomX = 0;
        int randomY = 0;
        float randomEdge = Noise(0, 0, RIVER_LAYER, seed);
        if (randomEdge < 0.25f) // LHS
        {
            upOffsetX = 0;
            upOffsetY = 1;
            upDiagOffsetX = 1;
            upDiagOffsetY = 1;
            fwdOffsetX = 1;
            fwdOffsetY = 0;
            downDiagOffsetX = 1;
            downDiagOffsetY = -1;
            downOffsetX = 0;
            downOffsetY = -1;

            int edgeLength = sourceTexture.height;
            randomX = 0;

            float noise = Noise(1, 1, RIVER_LAYER, seed);
            // 0 -> 0.99999

            float scaled = noise * edgeLength;
            // 0 -> 63.9999999

            float floor = Mathf.Floor(scaled);
            // 0 -> 63

            randomY = (int)floor;
        }
        else if (randomEdge < 0.5f) // TOP
        {
            upOffsetX = 1;
            upOffsetY = 0;

            upDiagOffsetX = 1;
            upDiagOffsetY = -1;

            fwdOffsetX = 0;
            fwdOffsetY = -1;

            downDiagOffsetX = -1;
            downDiagOffsetY = -1;

            downOffsetX = -1;
            downOffsetY = 0;

            int edgeLength = sourceTexture.width;
            randomY = sourceTexture.height - 1;

            float noise = Noise(1, 1, RIVER_LAYER, seed);
            // 0 -> 0.99999

            float scaled = noise * edgeLength;
            // 0 -> 63.9999999

            float floor = Mathf.Floor(scaled);
            // 0 -> 63

            randomX = (int)floor;
        }
        else if (randomEdge < 0.75f) // RHS
        {
            upOffsetX = 0;
            upOffsetY = -1;

            upDiagOffsetX = -1;
            upDiagOffsetY = -1;

            fwdOffsetX = -1;
            fwdOffsetY = 0;

            downDiagOffsetX = -1;
            downDiagOffsetY = 1;

            downOffsetX = 0;
            downOffsetY = 1;

            int edgeLength = sourceTexture.height;
            randomX = sourceTexture.width - 1;

            float noise = Noise(1, 1, RIVER_LAYER, seed);
            // 0 -> 0.99999

            float scaled = noise * edgeLength;
            // 0 -> 63.9999999

            float floor = Mathf.Floor(scaled);
            // 0 -> 63

            randomY = (int)floor;
        }
        else if (randomEdge < 1.0f) // BOT
        {
            upOffsetX = 1;
            upOffsetY = 0;

            upDiagOffsetX = 1;
            upDiagOffsetY = 1;

            fwdOffsetX = 0;
            fwdOffsetY = 1;

            downDiagOffsetX = -1;
            downDiagOffsetY = 1;

            downOffsetX = -1;
            downOffsetY = 0;

            int edgeLength = sourceTexture.width;
            randomY = 0;

            float noise = Noise(1, 1, RIVER_LAYER, seed);
            // 0 -> 0.99999

            float scaled = noise * edgeLength;
            // 0 -> 63.9999999

            float floor = Mathf.Floor(scaled);
            // 0 -> 63

            randomX = (int)floor;
        }

        // Generate random direction.
        {
            /*
            float random = Noise(2, 2, RIVER_LAYER, seed);
            // 0 -> 0.999
            float scaled = random * 5;
            // 0 -> 4.9999
            float floor = Mathf.Floor(scaled);
            // 0 -> 4
            int randomDirection = (int)floor;
            */


        }

        GeneratedRiverCell first = new GeneratedRiverCell();
        first.x = randomX;
        first.y = randomY;
        first.direction = 2;
        generatedRiverCells.AddLast(first);

        GeneratedRiverCell second = new GeneratedRiverCell();
        second.x = randomX + fwdOffsetX;
        second.y = randomY + fwdOffsetY;
        second.direction = 2;
        generatedRiverCells.AddLast(second);

        print(first.x);
        print(first.y);
        print(second.x);
        print(second.y);

        // 3. be a drunk emu
        GeneratedRiverCell current = second;
        int i = 0;
        while (current.x != 0 && current.x != 63 && current.y != 0 && current.y != 63)
        {
            ++i;

            GeneratedRiverCell previous = generatedRiverCells.Last.Value;
            current = new GeneratedRiverCell();

            // Generate random direction.
            float random = Noise(2 + i, 2 + i, RIVER_LAYER, seed);
            // 0 -> 0.999
            float scaled = random * 3;
            // 0 -> 4.9999
            float floor = Mathf.Floor(scaled);
            // 0 -> 4
            int randomDirection = (int)floor;

            current.direction = randomDirection;

            if (randomDirection == 0) // UP
            {
                current.x += previous.x + upOffsetX;
                current.y += previous.y + upOffsetY;
            }
            else if (randomDirection == 1) // UD
            {
                current.x += previous.x + upDiagOffsetX;
                current.y += previous.y + upDiagOffsetY;
            }
            else if (randomDirection == 1) // FWD
            {
                current.x += previous.x + fwdOffsetX;
                current.y += previous.y + fwdOffsetY;
            }
            else if (randomDirection == 3) // DD
            {
                current.x += previous.x + downDiagOffsetX;
                current.y += previous.y + downDiagOffsetY;
            }
            else if (randomDirection == 2) // DOWN
            {
                current.x += previous.x + downOffsetX;
                current.y += previous.y + downOffsetY;
            }

            print(current.x);
            print(current.y);

            generatedRiverCells.AddLast(current);
        }

        print(generatedRiverCells.Count);

        foreach (GeneratedRiverCell generatedRiverCell in generatedRiverCells)
        {
            sourceTexture.SetPixel(generatedRiverCell.x, generatedRiverCell.y, waterCell);
        }
        sourceTexture.Apply();
    }

    void GenerateVoxelHypemap()
    {
        for (float y = 0; y < sourceTexture.height; ++y)
        {
            for (float x = 0; x < sourceTexture.width; ++x)
            {
                float xCoord = seed + x / sourceTexture.width * scale;
                float yCoord = seed + y / sourceTexture.height * scale;
                float perlin = Mathf.PerlinNoise(xCoord, yCoord);

                perlin *= 10;
                // 0 -> 10

                int perlinClamped = (int)perlin;
                // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9

                perlin = perlinClamped / 10.0f;
                // 0 -> 1.0
                // 0.1 0.2 0.33


                sourceTexture.SetPixel((int)x, (int)y, new Color(perlin, perlin, perlin));
            }
        }
        sourceTexture.Apply();
    }

    void GenerateVoxelTerrain()
    {
        // delete existing voxels.

        if (voxels.Count != 0)
        {
            foreach (GameObject voxel in voxels)
            {
                GameObject.Destroy(voxel);
            }
            /*
            for(int voxelIndex = 0; voxelIndex < sourceTexture.width * sourceTexture.height; ++voxelIndex)
            {
                GameObject voxel = GameObject.Instantiate(voxelPrefab);
                voxels.Add(voxel);
            }
            */
        }

        for (float z = 0; z < sourceTexture.height; ++z)
        {
            for (float x = 0; x < sourceTexture.width; ++x)
            {
                float altitudeSample = sourceTexture.GetPixel((int)x, (int)z).r;

                float y = altitudeSample * heightScalar;

                GameObject voxel = GameObject.Instantiate(voxelPrefab);
                voxel.transform.position = new Vector3(x, y, z);

                voxels.Add(voxel);
            }
        }


        // sample each pixel
        // place each voxel based on that pixel
    }
}
