using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Referências às câmeras
    public Camera camera1; // Vista geral
    public Camera camera2; // Vista em plano médio
    public Camera camera3; // Primeira pessoa
    public Camera camera4; // Movimento dinâmico
    public Transform dynamicPath; // Caminho para a câmera dinâmica (array de pontos)

    public Transform orbitTarget; // Alvo para a câmera orbital
    public float orbitSpeed = 50f; // Velocidade da câmera orbital

    private int activeCameraIndex = 0;
    private float defaultFOV = 60f; // FOV padrão para todas as câmeras
    private float zoomFOV = 30f; // FOV para zoom
    private bool isZooming = false;

    private Vector3[] pathPoints; // Pontos de movimento dinâmico
    private int currentPathPoint = 0;

    private bool isTransitioning = false;
    private float transitionSpeed = 2f; // Velocidade da transição

    private Vector3 transitionStartPos;
    private Quaternion transitionStartRot;

    void Start()
    {
        // Inicializa o caminho dinâmico
        if (dynamicPath != null)
        {
            pathPoints = new Vector3[dynamicPath.childCount];
            for (int i = 0; i < dynamicPath.childCount; i++)
            {
                pathPoints[i] = dynamicPath.GetChild(i).position;
            }
        }

        SetActiveCamera(0); // Ativa a câmera inicial
    }

    void Update()
    {
        // Alterna entre as câmeras com teclas numéricas
        if (!isTransitioning)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetActiveCamera(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SetActiveCamera(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SetActiveCamera(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SetActiveCamera(3);
        }

        // Zoom para a câmera do plano médio
        if (activeCameraIndex == 1 && Input.GetKey(KeyCode.Z))
        {
            isZooming = true;
            camera2.fieldOfView = Mathf.Lerp(camera2.fieldOfView, zoomFOV, Time.deltaTime * 5f);
        }
        else if (activeCameraIndex == 1 && isZooming)
        {
            isZooming = false;
            camera2.fieldOfView = Mathf.Lerp(camera2.fieldOfView, defaultFOV, Time.deltaTime * 5f);
        }

        // Movimento da câmera dinâmica
        if (activeCameraIndex == 3 && pathPoints != null && pathPoints.Length > 0)
        {
            MoveDynamicCamera();
        }

        // Controles de câmera em primeira pessoa
        if (activeCameraIndex == 2)
        {
            FirstPersonControls();
        }

        // Câmera orbital
        if (activeCameraIndex == 0 && orbitTarget != null)
        {
            OrbitCamera();
        }
    }

    void SetActiveCamera(int cameraIndex)
    {
        if (cameraIndex != activeCameraIndex)
        {
            // Inicia transição suave
            transitionStartPos = GetActiveCamera().transform.position;
            transitionStartRot = GetActiveCamera().transform.rotation;
            StartCoroutine(SmoothTransition(cameraIndex));
        }
    }

    Camera GetActiveCamera()
    {
        switch (activeCameraIndex)
        {
            case 0: return camera1;
            case 1: return camera2;
            case 2: return camera3;
            case 3: return camera4;
            default: return camera1;
        }
    }

    System.Collections.IEnumerator SmoothTransition(int newCameraIndex)
    {
        isTransitioning = true;

        Camera nextCamera = GetCameraByIndex(newCameraIndex);
        Vector3 endPos = nextCamera.transform.position;
        Quaternion endRot = nextCamera.transform.rotation;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * transitionSpeed;
            Camera activeCam = GetActiveCamera();
            activeCam.transform.position = Vector3.Lerp(transitionStartPos, endPos, t);
            activeCam.transform.rotation = Quaternion.Lerp(transitionStartRot, endRot, t);
            yield return null;
        }

        // Ativa a câmera final e ajusta estado
        activeCameraIndex = newCameraIndex;
        isTransitioning = false;
        ActivateCamera(newCameraIndex);
    }

    Camera GetCameraByIndex(int index)
    {
        switch (index)
        {
            case 0: return camera1;
            case 1: return camera2;
            case 2: return camera3;
            case 3: return camera4;
            default: return camera1;
        }
    }

    void ActivateCamera(int cameraIndex)
    {
        camera1.gameObject.SetActive(false);
        camera2.gameObject.SetActive(false);
        camera3.gameObject.SetActive(false);
        camera4.gameObject.SetActive(false);

        GetCameraByIndex(cameraIndex).gameObject.SetActive(true);
    }

    void MoveDynamicCamera()
    {
        if (Vector3.Distance(camera4.transform.position, pathPoints[currentPathPoint]) < 0.1f)
        {
            currentPathPoint = (currentPathPoint + 1) % pathPoints.Length; // Próximo ponto
        }
        camera4.transform.position = Vector3.Lerp(camera4.transform.position, pathPoints[currentPathPoint], Time.deltaTime);
    }

    void FirstPersonControls()
    {
        float mouseX = Input.GetAxis("Mouse X") * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * 100f * Time.deltaTime;

        camera3.transform.Rotate(Vector3.up * mouseX, Space.World);
        camera3.transform.Rotate(Vector3.left * mouseY, Space.Self);
    }

    void OrbitCamera()
    {
        camera1.transform.RotateAround(orbitTarget.position, Vector3.up, orbitSpeed * Time.deltaTime);
    }
}

