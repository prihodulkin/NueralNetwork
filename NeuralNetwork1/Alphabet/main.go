package main

import (
	"flag"
	"fmt"
	"github.com/disintegration/imaging"
	"image"
	"image/color"
	"image/jpeg"
	_ "image/jpeg"
	"image/png"
	"io"
	"log"
	"math"
	"math/rand"
	"os"
	"path/filepath"
	"strings"
)

type ConfigSlice struct {
	Width, Height int
	Border        float64
	File          string
	Names         string
	Offset        int
}

type Command interface {
	Run()
}

func (config ConfigSlice) Run() {
	flag.IntVar(&config.Height, "height", 12, "Height in cells")
	flag.IntVar(&config.Offset, "offset", 10, "Offset in numeration of pictures")
	flag.Float64Var(&config.Border, "border", 0.06, "Border in percent from cell")
	flag.StringVar(&config.File, "file", "input/*.jpg", "Files (can be pattern, like as *.jpg)")
	flag.StringVar(&config.Names, "names", "open,stop,pause,next,forwardstep,io,play,forwardrewind", "Width name")
	flag.Parse()
	fileNum := 0
	err := filepath.Walk(".", func(path string, info os.FileInfo, err error) error {
		matched, err := filepath.Match(filepath.Clean(config.File), path)
		if err != nil {
			log.Println(path, err)
			return nil
		}
		if !info.IsDir() && matched {
			file, err := os.Open(path)
			log.Println(path, matched, fileNum*config.Height)
			if err != nil {
				log.Fatalln(err)
			}
			imgr, s, err := image.Decode(file)
			log.Println(s)
			if err != nil {
				log.Fatalln(err)
			}
			img := imgr.(SubImage)
			labels := strings.Split(config.Names, ",")
			config.Width = len(labels)
			cellWidth := img.Bounds().Max.X / config.Width
			cellHeight := img.Bounds().Max.Y / config.Height
			os.Mkdir("output", 0666)
			bounds := int(math.Min(float64(cellHeight), float64(cellWidth))*config.Border) / 2
			for j := 0; j < config.Height; j++ {
				for i, name := range labels {
					output, err := os.Create(filepath.Clean(fmt.Sprintf("%s%s%.3d.%s", "output/", name, config.Offset+j+fileNum*config.Height, "png")))
					if err != nil {
						log.Fatalln(err)
					}
					rect := image.Rect(i*cellWidth+bounds, j*cellHeight+bounds, (i+1)*cellWidth-bounds, (j+1)*cellHeight-bounds)
					err = png.Encode(output, img.SubImage(rect))
					if err != nil {
						log.Fatalln(rect, err)
					}
					_ = output.Close()
				}
			}
			fileNum++
		}
		return nil
	})
	if err != nil {
		log.Fatal(err)
	}
}

type ConfigRotate struct {
	File  string
	Name  string
	Count int
}

func (config ConfigRotate) Rotate(img image.Image) image.Image {
	switch config.Count % 4 {
	case 1:
		return imaging.Rotate90(img)
	case 2:
		return imaging.Rotate180(img)
	case 3:
		return imaging.Rotate270(img)
	}
	return img
}

func (config ConfigRotate) Run() {
	flag.IntVar(&config.Count, "rotate", 2, "1 - 90 degrees, 2 - 180 degrees, 3 - 270 degrees")
	flag.StringVar(&config.File, "file", "output/forwardrewind", "Prefix")
	flag.StringVar(&config.Name, "name", "output/backwardrewind", "New prefix")
	flag.Parse()
	err := filepath.Walk(".", func(path string, info os.FileInfo, err error) error {
		matched := strings.HasPrefix(path, filepath.Clean(config.File))
		if !info.IsDir() && matched {
			file, err := os.Open(path)
			log.Println(path, matched)
			if err != nil {
				log.Fatalln(err)
			}
			img, s, err := image.Decode(file)
			if err != nil {
				log.Fatalln(err)
			}
			file.Close()
			newName := config.Name + strings.TrimPrefix(path, filepath.Clean(config.File))
			output, err := os.Create(filepath.Clean(newName))
			if err != nil {
				log.Fatalln(err)
			}
			Save(s, output, config.Rotate(img))
			output.Close()
		}
		return nil
	})
	if err != nil {
		log.Fatal(err)
	}
}

type ConfigScale struct {
	File  string
	Name  string
}

func Random(from, to float64) float64 {
	return rand.Float64()*(to-from) + from
}

func RandomScale(what int, from, to float64) int {
	r := Random(from, to)
	if rand.Int()%2 == 0 {
		r = 1 / r
	}
	return int(float64(what) * r)
}

func Save(s string, to io.Writer, img image.Image) {
	switch s {
	case "png":
		png.Encode(to, img)
	case "jpg", "jpeg":
		jpeg.Encode(to, img, nil)
	default:
		panic(s)
	}
}

func (config ConfigScale) Run() {
	flag.StringVar(&config.File, "file", "Theta/", "Prefix")
	flag.StringVar(&config.Name, "name", "Theta/new", "New prefix")
	flag.Parse()
	for _, directory := range []string{
		"Theta/",
	} {
		config.File = fmt.Sprint(directory, 0)
		config.Name = fmt.Sprint(directory)
		id := 1
		err := filepath.Walk(directory, func(path string, info os.FileInfo, err error) error {
			if !info.IsDir() {
				file, err := os.Open(path)
				log.Println(path)
				if err != nil {
					log.Fatalln(err)
				}
				img, s, err := image.Decode(file)
				if err != nil {
					log.Fatalln(err)
				}
				file.Close()
				id++
				for i := 1; i < 5; i++ {
					newName := config.Name + fmt.Sprint(i)+"_"+fmt.Sprint(id) + ".png"
					output, err := os.Create(filepath.Clean(newName))
					if err != nil {
						log.Fatalln(err)
					}
					newImage := imaging.Resize(img, img.Bounds().Max.X, RandomScale(img.Bounds().Max.Y, 1.1, 1.3), imaging.CatmullRom)
					Save(s, output, imaging.Rotate(newImage, Random(-30, 30), color.White))
					output.Close()
				}
			}
			return nil
		})
		if err != nil {
			log.Fatal(err)
		}
	}
}

type SubImage interface {
	image.Image
	SubImage(image.Rectangle) image.Image
}

func main() {
	commands := map[string]Command{
		"slice":  ConfigSlice{},
		"rotate": ConfigRotate{},
		"scaleAndRotate":  ConfigScale{},
	}
	commands["scaleAndRotate"].Run()
}
