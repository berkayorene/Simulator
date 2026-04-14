import os

def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    output_file = os.path.join(script_dir, "tum_scriptler.txt")

    print(f"Scriptin olduğu klasör: {script_dir}")

    found_any = False

    with open(output_file, "w", encoding="utf-8") as outfile:
        for filename in os.listdir(script_dir):
            if filename.endswith(".cs"):
                found_any = True
                outfile.write(f"\n\n--- Dosya: {filename} ---\n\n")
                file_path = os.path.join(script_dir, filename)

                try:
                    with open(file_path, "r", encoding="utf-8") as infile:
                        outfile.write(infile.read())
                except UnicodeDecodeError:
                    print(f"[UYARI] {filename} UTF-8 değil, Latin-1 ile tekrar okunuyor.")
                    with open(file_path, "r", encoding="latin-1") as infile:
                        outfile.write(infile.read())

                outfile.write("\n\n")

    if found_any:
        print(f"Tüm .cs dosyaları '{output_file}' dosyasına kaydedildi.")
    else:
        print("Hiç .cs dosyası bulunamadı!")

    input("\nKapatmak için Enter'a bas...")

if __name__ == "__main__":
    main()
