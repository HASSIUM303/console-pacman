package main

import (
	"encoding/json"
	"fmt"
	"ghost-services/queue"
	"log"
	"net/http"
	"os"
)

type PathRequest struct {
	GhostX    int     `json:"ghostX"`
	GhostY    int     `json:"ghostY"`
	PacmanX   int     `json:"pacmanX"`
	PacmanY   int     `json:"pacmanY"`
	Map       [][]int `json:"map"`
	MapWidth  int     `json:"mapWidth"`
	MapHeight int     `json:"mapHeight"`
}

type PathResponse struct {
	NextX int  `json:"nextX"`
	NextY int  `json:"nextY"`
	Found bool `json:"found"`
}

type HealthResponse struct {
	Status string `json:"status"`
}

func bfsFindPath(req PathRequest) (int, int, bool) {
	if req.GhostX == req.PacmanX && req.GhostY == req.PacmanY {
		return req.GhostX, req.GhostY, true
	}

	dirs := []struct{ dx, dy int }{
		{0, -1},
		{0, 1},
		{-1, 0},
		{1, 0},
	}

	visited := make([][]bool, req.MapHeight)
	parent := make([][]struct{ px, py int }, req.MapHeight)

	for i := range visited {
		visited[i] = make([]bool, req.MapWidth)
		parent[i] = make([]struct{ px, py int }, req.MapWidth)
	}

	q := queue.New()
	q.Enqueue(req.GhostX, req.GhostY)
	visited[req.GhostY][req.GhostX] = true

	for !q.IsEmpty() {
		x, y := q.Dequeue()

		if x == -1 && y == -1 {
			break
		}

		if x == req.PacmanX && y == req.PacmanY {
			return restorePath(req.GhostX, req.GhostY, req.PacmanX, req.PacmanY, parent)
		}

		for _, d := range dirs {
			nx, ny := x+d.dx, y+d.dy

			if nx >= 0 && nx < req.MapWidth && ny >= 0 && ny < req.MapHeight && !visited[ny][nx] && req.Map[ny][nx] == 0 {
				visited[ny][nx] = true
				parent[ny][nx] = struct{ px, py int }{x, y}
				q.Enqueue(nx, ny)
			}
		}
	}

	return req.GhostX, req.GhostY, false
}

func restorePath(sx, sy, tx, ty int, parent [][]struct{ px, py int }) (int, int, bool) {
	cx, cy := tx, ty

	if cx == sx && cy == sy {
		return sx, sy, false
	}

	for cx != sx || cy != sy {
		p := parent[cy][cx]
		if p.px == sx && p.py == sy {
			return cx, cy, true
		}
		cx, cy = p.px, p.py
	}

	return sx, sy, false
}

func handlePath(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Метод не доступен", http.StatusMethodNotAllowed)
		return
	}

	var req PathRequest

	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	nextX, nextY, found := bfsFindPath(req)

	resp := PathResponse{
		NextX: nextX,
		NextY: nextY,
		Found: found,
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(resp)
}

func handleHealth(w http.ResponseWriter, r *http.Request) {
	resp := HealthResponse{Status: "ok"}
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(resp)
}

func main() {
	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	http.HandleFunc("/path", handlePath)
	http.HandleFunc("/health", handleHealth)

	fmt.Printf("Сервис логики призраков запущен по порту: %s\n", port)
	log.Fatal(http.ListenAndServe(":"+port, nil))
}
	