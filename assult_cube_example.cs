namespace video_game_cheat_tester
{
    public partial class f_main : Form
    {
        mem_help32 mem = new mem_help32("ac_client");
        uint target_ammo;
        public f_main()
        {
            InitializeComponent();
        }

        private void f_main_Load(object sender, EventArgs e)
        {
            uint based = mem.get_base_address(0x0010A280);
            int[] offsets_ammo = { 0x8, 0x278, 0x34, 0x4E0 };
            target_ammo = mem_utils.offset_calculator(mem, based, offsets_ammo);
            for (;;)
                mem.write_mem<int>(target_ammo, 20);
            // mem.
        }
    }
}
