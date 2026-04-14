import os

def main():
    # Scriptin bulunduğu klasör
    script_dir = os.path.dirname(os.path.abspath(__file__)) # current directory 
    output_file = os.path.join(script_dir, "tum_scriptler.txt")

    print(f"Scriptin olduğu klasör: {script_dir}")

    found_any = False

    with open(output_file, "w", encoding="utf-8") as outfile:
        for filename in os.listdir(script_dir):
            if filename.endswith(".cs"):
                found_any = True
                outfile.write(f"\n\n--- Dosya: {filename} ---\n\n")
                with open(os.path.join(script_dir, filename), "r", encoding="utf-8") as infile:
                    outfile.write(infile.read())
                outfile.write("\n\n")

    if found_any:
        print(f"Tüm .cs dosyaları '{output_file}' dosyasına kaydedildi.")
    else:
        print("Hiç .cs dosyası bulunamadı!")

    input("\nKapatmak için Enter'a bas...")

if __name__ == "__main__":
    main()
